using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Game
    {
        const int MIN_GAME_SIZE = 5;
        const int MAX_GAME_SIZE = 10;

        public enum State
        {
            GameSetup,
            Playing,
            EvilTriumphs,
            GoodPrevails,
            GuessingMerlin,
            MerlinDies
        }

        public Game()
        {
            Players = new List<Player>();
            Rounds = new List<Round>();
            AvailableCharacters = new List<Character>();

            LadyOfTheLakeUses = new List<LadyOfTheLakeUse>();
            ExcaliburUses = new List<ExcaliburUse>();
            LoyaltyDeck = new List<LoyaltyCard> { LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.SwitchAlegiance, LoyaltyCard.SwitchAlegiance };

            Random random = new Random();
            LoyaltyDeck = LoyaltyDeck.Shuffle().ToList();

            //standard rules
			Rules = new List<Rule>()
			{
				Rule.IncludeLadyOfTheLake,
			};
        }

        public int GameId { get; set; }
        public List<Character> AvailableCharacters { get; set; }
        public List<PlayerMessage> Messages { get; set; }

        public List<Player> Players { get; set; }
        public List<Round> Rounds { get; set; }
        public int QuestIndicator { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }

        public List<LadyOfTheLakeUse> LadyOfTheLakeUses { get; set; }
        public List<ExcaliburUse> ExcaliburUses { get; set; }
        public Player HolderOfLadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }

		public List<Rule> Rules { get; set; }  

        public int GameSize { get { return Players.Count; } }
        public List<RoundTable> RoundTables
        {
            get
            {
                var roundTables = new List<RoundTable>();
                switch (GameSize)
                {
                    case 5:
                        roundTables.Add(new RoundTable(2));
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(2));
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(3));
                        break;
                    case 6:
                        roundTables.Add(new RoundTable(2));
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(4));
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(4));
                        break;
                    case 7:
                        roundTables.Add(new RoundTable(2));
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(4, 2));
                        roundTables.Add(new RoundTable(4));
                        break;
                    case 8:
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(4));
                        roundTables.Add(new RoundTable(4));
                        roundTables.Add(new RoundTable(5, 2));
                        roundTables.Add(new RoundTable(5));
                        break;
                    case 9:
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(4));
                        roundTables.Add(new RoundTable(4));
                        roundTables.Add(new RoundTable(5, 2));
                        roundTables.Add(new RoundTable(5));
                        break;
                    case 10:
                        roundTables.Add(new RoundTable(3));
                        roundTables.Add(new RoundTable(4));
                        roundTables.Add(new RoundTable(4));
                        roundTables.Add(new RoundTable(5, 2));
                        roundTables.Add(new RoundTable(5));
                        break;
                }
                return roundTables;
            }
        }

        public void UseLadyOfTheLake(Player player, Player target)
        {
            if (HolderOfLadyOfTheLake != player)
                throw new Exception("Hax. Player does not have lady of the lake.");

			LadyOfTheLakeUses.Add(new LadyOfTheLakeUse { UsedBy = player, UsedOn = target, ResultWasEvil = IsCharacterEvil(target.Character), UsedOnRoundNumber = Rounds.Count + 1 });

            OnLadyOfTheLakeUsed();
        }

        public void GuessMerlin(Player player, Player guess)
        {
            if (player.Character != Character.Assassin)
                throw new Exception("Hax. Player is not assassin.");

            if (AssassinsGuessAtMerlin != null)
                throw new Exception("Hax. Assassin has already guessed.");

            AssassinsGuessAtMerlin = guess;

            OnMerlinGuessedAt();
        }

        private void OnMerlinGuessedAt()
        {
        }

        private void OnAfterAction()
        {

        }

        public void AddCharacter(Character character)
        {
            if (DetermineState() != State.GameSetup)
                throw new Exception("Can only add characters during setup");

            AvailableCharacters.Add(character);
            OnCharacterAddedOrPlayerJoined();
        }

        public void SetCharacter(int index, Character character)
        {
            if (DetermineState() != State.GameSetup)
                throw new Exception("Can only change characters during setup");

            AvailableCharacters[index] = character;         
        }


        private void OnCharacterAddedOrPlayerJoined()
        {
            if (AvailableCharacters.Count < Players.Count)
                AddCharacter(Character.UnAllocated);
        }

        public void StartGame()
        {
            //check that game hasn't already started
            if (Rounds.Count > 0)
            {
                return;
            }
            //check if game ready to start
            if (AvailableCharacters.Count == GameSize)
            {
                AllocateCharactersToPlayers();
            }
        }

        public Guid JoinGame(string playerName, Guid playerGuid)
        {
            playerName = playerName.Uniquify(Players.Select(p => p.Name));

            Players.Add(new Player() { Name = playerName, Guid = playerGuid });

            OnCharacterAddedOrPlayerJoined();

            OnAfterAction();

            return playerGuid;
        }

        private void AllocateCharactersToPlayers()
        {
            //on last player, allocate characters
                if (AvailableCharacters.Count != GameSize)
                    throw new Exception("Not Enough Characters for Players");

                var characterCards = AvailableCharacters.ToList();
                Random random = new Random();
                foreach (var player in Players)
                {
                    var index = random.Next(characterCards.Count);
                    player.Character = characterCards[index];
                    characterCards.RemoveAt(index);
                }

                OnGameStart();
            }

        private void OnGameStart()
        {
            //create first round
            var leader = new Random().Next(GameSize);
            if (Rules.Contains(Rule.IncludeLadyOfTheLake))
            {
                HolderOfLadyOfTheLake = Players[(leader + GameSize - 1) % GameSize];
            }
            CreateRound(leader);
        }

        private void CreateRound(int leader)
        {
            if (Rounds.Count > RoundTables.Count)
                throw new Exception("round overrun");

            var tableaus = RoundTables[Rounds.Count];
            Rounds.Add(new Round(Players, leader, tableaus.TeamSize, tableaus.RequiredFails));
        }

        public Round CurrentRound { get { return Rounds.LastOrDefault(); } }

        public void AddToTeam(Player player, Player proposedPlayer)
        {
            CurrentRound.AddToTeam(player, proposedPlayer);
        }

        public void AssignExcalibur(Player player, Player proposedPlayer)
        {
            if (!Rules.Contains(Rule.IncludeExcalibur))
            {
                throw new Exception("Game does not include excalibur");
            }

            CurrentRound.AssignExcalibur(player, proposedPlayer);
        }

        public void UseExcalibur(Player player, Player proposedPlayer)
        {
            if (!Rules.Contains(Rule.IncludeExcalibur))
            {
                throw new Exception("Game does not include excalibur");
            }

            if (proposedPlayer != null)
            {
                var originalMission = CurrentRound.UseExcalibur(player, proposedPlayer);
                ExcaliburUses.Add(new ExcaliburUse { UsedBy = player, UsedOn = proposedPlayer, OriginalMissionWasSuccess = originalMission, UsedOnRoundNumber = Rounds.Count + 1 });
            }
        }

        public void VoteForTeam(Player player, bool approve)
        {
            CurrentRound.VoteForTeam(player, approve);
        }

        public void SubmitQuest(Player player, bool success)
        {
            if (Rules.Contains(Rule.GoodMustAlwaysVoteSucess) && !success && !IsCharacterEvil(player.Character))
            {
                throw new Exception("Good must always vote success");
            }
            if (Rules.Contains(Rule.LancelotsMustVoteFanatically) && (player.Character == Character.Lancelot || player.Character == Character.EvilLancelot))
            {
                if ((success && IsCharacterEvil(player.Character)) || (!success && !IsCharacterEvil(player.Character))) 
                {
                    throw new Exception("Lancelot must move fanatically");
                }
            }
            CurrentRound.SubmitQuest(player, success);

            if (CurrentRound.CurrentTeam.Quests.Count == CurrentRound.TeamSize)
            {
                OnLastQuestCard();
            }
        }

        private void OnLastQuestCard()
        {
            //on last quest submit, create the next round
            var roundState = CurrentRound.DetermineState();
            if (roundState == Round.State.Succeeded || roundState == Round.State.Failed)
            {
                OnEndOfRound(Rounds.Count);
            }
        }

        private void OnEndOfRound(int roundNumber)
        {
            //3 failed missions, don't bother going any further
            if (Rounds.Where(r => r.DetermineState() == Round.State.Failed).Count() >= 3)
                return;

            //3 successful missions, don't bother going any further
            if (Rounds.Where(r => r.DetermineState() == Round.State.Succeeded).Count() >= 3)
                return;

            if (roundNumber >= 2 && Rules.Contains(Rule.IncludeLadyOfTheLake))
            {
                //wait for lady of the lake to be used
                return;
            }

            OnStartNextRound(roundNumber + 1);
        }

        private void OnLadyOfTheLakeUsed()
        {
            HolderOfLadyOfTheLake = LadyOfTheLakeUses.Last().UsedOn;
            OnStartNextRound(Rounds.Count + 1);
        }

        private void OnStartNextRound(int roundNumber)
        {
            //create the next round
            CreateRound(CurrentRound.NextPlayer);

            var loyaltyCard = GetLoyaltyCard(roundNumber);
            if (loyaltyCard == LoyaltyCard.SwitchAlegiance)
            {
                LancelotAllegianceSwitched = !LancelotAllegianceSwitched;
            }
        }


        public State DetermineState()
        {
            if (AvailableCharacters.Count != Players.Count || Players.Count == 0 || Players.Any(i => i.Character == Character.UnAllocated) || AvailableCharacters.Any(i => i == Character.UnAllocated))
                return State.GameSetup;

            if (Rounds.Where(r => r.DetermineState() == Round.State.Failed).Count() >= 3)
                return State.EvilTriumphs;

            if (Rounds.Where(r => r.DetermineState() == Round.State.Succeeded).Count() >= 3)
            {
				if (Rules.Contains(Rule.IncludeLadyOfTheLake) && LadyOfTheLakeUses.Count < Rounds.Count - 2)
                    return State.Playing;

                if (AssassinsGuessAtMerlin == null && Players.Any(p => p.Character == Character.Merlin) && Players.Any(p => p.Character == Character.Assassin))
                    return State.GuessingMerlin;

                if (AssassinsGuessAtMerlin != null && AssassinsGuessAtMerlin.Character == Character.Merlin)
                    return State.MerlinDies;

                return State.GoodPrevails;
            }

            return State.Playing;
        }

        /// <summary>
        /// this is the available actions a player has given who they are and the state of the game
        /// this should show a list of buttons on the webpage or something
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<Action.Type> AvailableActions(Player player)
        {
            var gameState = DetermineState();
            switch (gameState)
            {
                case Game.State.Playing:
                    var roundState = CurrentRound.DetermineState();
                    var quest = CurrentRound.CurrentTeam;
                    switch (roundState)
                    {
                        case Round.State.ProposingPlayers:
                            if (player != null && quest.Leader.Name == player.Name)
                            {
                                return new List<Action.Type>() { Action.Type.AddToTeam, Action.Type.Message };
                                //todo assign excalibur if rule turned on and team assigned (need another round state?)
                            }
                            return new List<Action.Type>() { Action.Type.Message };
                        case Round.State.Voting:
                            if (player != null && !quest.Votes.Select(v => v.Player.Name).ToList().Contains(player.Name))
                            {
                                return new List<Action.Type>() { Action.Type.VoteForTeam, Action.Type.Message };
                            }
                            return new List<Action.Type>() { Action.Type.Message };
                        case Round.State.Questing:
                            if (player != null && quest.TeamMembers.Select(v => v.Name).ToList().Contains(player.Name) &&
                                !quest.Quests.Select(q => q.Player.Name).ToList().Contains(player.Name))
                            {
                                return new List<Action.Type>() { Action.Type.Message, Action.Type.SubmitQuestCard };
                            }
                            return new List<Action.Type>() { Action.Type.Message };
                    }

                    //todo use excalibur if rule turned on and has excalibur assigned 
                    //todo player can pick not to use it
                    //rule contains IncludeExcalibur and ExcaliburUses.all(!= currentRound)

                    //round over but still current
					if (Rules.Contains(Rule.IncludeLadyOfTheLake) && Rounds.Count >= 2 && HolderOfLadyOfTheLake == player)
                    {
                        return new List<Action.Type>() { Action.Type.UseTheLadyOfTheLake, Action.Type.Message };
                    }

                    return new List<Action.Type>();

                case Game.State.GameSetup:
                    var actions = new List<Action.Type>();
					if (Players.Count == AvailableCharacters.Count && Players.Count >= MIN_GAME_SIZE && Players.Count <= MAX_GAME_SIZE)
					{
						actions.Add(Action.Type.StartGame);
					}

					if (player == null && Players.Count < MAX_GAME_SIZE)
					{
						actions.Add(Action.Type.JoinGame);
					}

					if (Players.Count < MAX_GAME_SIZE)
					{
						actions.Add(Action.Type.AddBot);
					}

					actions.Add(Action.Type.AddRule);
                    return actions;

                case Game.State.GuessingMerlin:
                    if (player != null && player.Character == Character.Assassin)
                        return new List<Action.Type>() { Action.Type.GuessMerlin, Action.Type.Message };
                    return new List<Action.Type>() { Action.Type.Message };

                case Game.State.EvilTriumphs:
                case Game.State.GoodPrevails:
                case Game.State.MerlinDies:
                    return new List<Action.Type>() { Action.Type.Message };
            }
            return new List<Action.Type>();

        }

        /// <summary>
        /// once a player performs an action, this should update the game state appropriately
        /// </summary>
        /// <param name="player"></param>
        /// <param name="action"></param>
        public void PerformAction(Player player, Action action)
        {
            if (!AvailableActions(player).Contains(action.ActionType))
                throw new Exception(String.Format("Hax. Player {0} can't perform action {1}", player.Name, action));

            switch (action.ActionType)
            {
				case Action.Type.AddRule:
					AddRule(action.Rule);
					break;
                //case Action.Type.AddCharacter:
                //    AddCharacter(action.Character);
                //    break;
                case Action.Type.GuessMerlin:
                    GuessMerlin(player, action.Player);
                    break;
                case Action.Type.JoinGame:
                    JoinGame(action.Name, Guid.NewGuid());
                    break;
                case Action.Type.AddToTeam:
                    AddToTeam(player, action.Player);
                    break;
                case Action.Type.AssignExcalibur:
                    AssignExcalibur(player, action.Player);
                    break;
                case Action.Type.UseExcalibur:
                    UseExcalibur(player, action.Player);
                    break;
                case Action.Type.SubmitQuestCard:
                    SubmitQuest(player, action.Success);
                    break;
                case Action.Type.VoteForTeam:
                    VoteForTeam(player, action.Accept);
                    break;
                case Action.Type.UseTheLadyOfTheLake:
                    UseLadyOfTheLake(player, action.Player);
                    break;
                case Action.Type.Message:
                    Message(player, action.Message);
                    break;
                default:
                    throw new NotImplementedException();
            }

            OnAfterAction();
        }

		public void AddRule(Rule rule)
		{
			//if adding twice, then remove. bit hax but useful for accidents
			if (Rules.Contains(rule))
			{
				Rules.Remove(rule);
			}
			else
			{
				Rules.Add(rule);
			}
		}

        private void Message(Player player, string message)
        {
            CurrentRound.CurrentTeam.Messages.Add(new PlayerMessage { Player = player, Message = message });
        }

        public bool IsCharacterEvil(Character character)
        {
            switch (character)
            {
                case Core.Character.Assassin:
                case Core.Character.MinionOfMordred:
                case Core.Character.Mordred:
                case Core.Character.Morgana:
                case Core.Character.Oberon:
                    return true;
                case Core.Character.Lancelot:
                    if (LancelotAllegianceSwitched)
                    {
                        return true;
                    }
                    break;
                case Core.Character.EvilLancelot:
                    if (!LancelotAllegianceSwitched)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        public Knowledge PlayerKnowledge(Player myself, Player someoneelse)
        {
            if (myself == null)
                return Knowledge.Player;

			if (Rules.Contains(Rule.LancelotsKnowEachOther))
            {
                if ((myself.Character == Character.Lancelot || myself.Character == Character.EvilLancelot) && (someoneelse.Character == Character.Lancelot))
                {
                    return Knowledge.Lancelot;
                }
                if ((myself.Character == Character.Lancelot || myself.Character == Character.EvilLancelot) && (someoneelse.Character == Character.EvilLancelot))
                {
                    return Knowledge.EvilLancelot;
                }
            }

            var ladyofthelake = LadyOfTheLakeUses.FirstOrDefault(u => u.UsedBy == myself && u.UsedOn == someoneelse);
            if (ladyofthelake != null)
            {
                return IsCharacterEvil(ladyofthelake.UsedOn.Character) ? Knowledge.Evil : Knowledge.Good;
            }

            if (DetectEvil(myself, someoneelse))
            {
                return Knowledge.Evil;
            }

            if (DetectMagic(myself, someoneelse))
            {
                return Knowledge.Magical;
            }

            return Knowledge.Player;
        }


        /// <summary>
        /// does myself know someoneelse is evil. e.g. they are both minions, or myself is merlin
        /// </summary>
        /// <param name="playerSelf"></param>
        /// <param name="playerTarget"></param>
        /// <returns></returns>
        public static bool DetectEvil(Player myself, Player someoneelse)
        {
            if (myself == null)
                return false;

            //minions know each other (except oberon)
            if (myself.Character == Character.Assassin || myself.Character == Character.Morgana || myself.Character == Character.MinionOfMordred || myself.Character == Character.Mordred)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morgana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Mordred || someoneelse.Character == Character.EvilLancelot)
                {
                    return true;
                }
            }

            //merlin knows minions (except mordred)
            if (myself.Character == Character.Merlin)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morgana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Oberon)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// does myself know someoneelse is merlin (or morcana), e.g. myself is percival
        /// </summary>
        /// <param name="playerSelf"></param>
        /// <param name="playerTarget"></param>
        /// <returns></returns>
        public static bool DetectMagic(Player myself, Player someoneelse)
        {
            if (myself == null)
                return false;

            if (myself.Character == Character.Percival)
            {
                if (someoneelse.Character == Character.Merlin || someoneelse.Character == Character.Morgana)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsLancelot()
        {
            return (AvailableCharacters.Contains(Character.EvilLancelot) || AvailableCharacters.Contains(Character.Lancelot));
        }

        public LoyaltyCard? GetLoyaltyCard(int roundNumber)
        {
			if (Rules.Contains(Rule.LoyaltyCardsDeltInAdvance))
                return null;
            if (!ContainsLancelot())
                return null;
            return LoyaltyDeck[roundNumber - 1];
        }
    }
}
