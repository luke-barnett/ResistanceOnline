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
            Setup,
            ChoosingTeam,
            AssigningExcalibur,
            VotingForTeam,
            VoteFailed,
            Questing,
            UsingExcalibur,
            LadyOfTheLake,
            GuessingMerlin,
            EvilTriumphs,
            EternalChaos,
            GoodPrevails,
        }

        public State GameState { get; set; }
        public int GameId { get; set; }
        public List<Character> AvailableCharacters { get; set; }
        public List<PlayerMessage> Messages { get; set; }
        public List<Player> Players { get; set; }
        public List<Round> Rounds { get; set; }
        public int QuestIndicator { get; set; }
        public Player HolderOfLadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }
        public List<Rule> Rules { get; set; }
        public int GameSize { get { return Players.Count; } }
        public Player AssassinsGuessAtMerlin { get; set; }

        public Game()
        {
            GameState = State.Setup;
            Players = new List<Player>();
            Rounds = new List<Round>();
            AvailableCharacters = new List<Character>();
            LoyaltyDeck = new List<LoyaltyCard> { LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.SwitchAlegiance, LoyaltyCard.SwitchAlegiance }.Shuffle().ToList();

            //standard rules
			Rules = new List<Rule>()
			{
				Rule.IncludeLadyOfTheLake,
			};
        }


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

        public void GuessMerlin(Player player, Player guess)
        {
            if (GameState != State.GuessingMerlin)
            {
                throw new Exception("Hax. You shouldn't be guessing merlin at this stage");
            }

            if (player.Character!=Character.Merlin)
            {
                throw new Exception("Hax. Player is not assassin.");
            }

            AssassinsGuessAtMerlin = guess;

            if (guess.Character == Character.Merlin)
            {
                GameState = State.EvilTriumphs;
            }
            else
            {
                GameState = State.GoodPrevails;
            }
        }

        public void SetCharacter(int index, Character character)
        {
            if (GameState != State.Setup)
                throw new Exception("Can only change characters during setup");

            AvailableCharacters[index] = character;         
        }

        public void StartGame()
        {
            if (GameState != State.Setup)
                throw new Exception("Can only start game during setup");

            //check that game hasn't already started
            if (Rounds.Count > 0)
            {
                return;
            }

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

            var holderOfLadyOfTheLake = Players.Random();
            var leader = Players.Next(holderOfLadyOfTheLake);

            //create first round
            NextRound(leader);
        }

        public Guid JoinGame(string playerName, Guid playerGuid)
        {
            if (String.IsNullOrWhiteSpace(playerName))
                playerName = String.Empty;
            playerName = playerName.Uniquify(Players.Select(p => p.Name));

            Players.Add(new Player() { Name = playerName, Guid = playerGuid });

            var evilCount = AvailableCharacters.Count(c=> IsCharacterEvil(c, false));
            if (evilCount < (AvailableCharacters.Count / 3.0))
            {
                AvailableCharacters.Add(Character.MinionOfMordred);
            }
            else
            {
                AvailableCharacters.Add(Character.LoyalServantOfArthur);
            }            

            return playerGuid;
        }

        public Round CurrentRound { get { return Rounds.LastOrDefault(); } }
        public Team CurrentTeam { get { return CurrentRound.CurrentTeam; } }      


        private void NextRound(Player leader)
        {
            var roundTable = RoundTables[Rounds.Count];
            var round = new Round(Players, leader, HolderOfLadyOfTheLake, roundTable.TeamSize, roundTable.RequiredFails, LancelotAllegianceSwitched);
            var team = new Team(leader, roundTable.TeamSize, roundTable.RequiredFails);
            round.Teams.Add(team);
            Rounds.Add(round);
            GameState = State.ChoosingTeam;
        }

        void OnRoundFinished()
        {
            //3 failed missions, don't bother going any further
            if (Rounds.Count(r => !r.IsSuccess.Value) >= 3)
            {
                GameState = State.EvilTriumphs;
                return;
            }

            //3 successful missions, don't bother going any further
            if (Rounds.Count(r => r.IsSuccess.Value) >= 3)
            {
                if (AvailableCharacters.Contains(Character.Assassin))
                {
                    GameState = State.GuessingMerlin;
                }
                else
                {
                    GameState = State.GoodPrevails;
                }
                return;
            }

            //loyalty cards            
            var loyaltyCard = GetLoyaltyCard(Rounds.Count);
            if (loyaltyCard == LoyaltyCard.SwitchAlegiance)
            {
                LancelotAllegianceSwitched = !LancelotAllegianceSwitched;
            }

            NextRound(Players.Next(CurrentRound.CurrentTeam.Leader));
        }

        public List<Action.Type> AvailableActions(Player player)
        {
            switch (GameState)
            {
                case State.ChoosingTeam:
                    if (player == CurrentTeam.Leader)
                    {
                        return new List<Action.Type>() { Action.Type.AddToTeam };
                    }
                    break;
                case State.AssigningExcalibur:
                    if (player == CurrentTeam.Leader)
                    {
                        return new List<Action.Type>() { Action.Type.AssignExcalibur };
                    }
                    break;
                case State.VotingForTeam:
                    if (!CurrentTeam.Votes.Any(v => v.Player == player))
                    {
                        return new List<Action.Type>() { Action.Type.VoteForTeam };
                    }
                    break;
                case State.Questing:
                    if (CurrentTeam.TeamMembers.Contains(player) && !CurrentTeam.Quests.Any(q => q.Player == player))
                    {
                        return new List<Action.Type>() { Action.Type.SubmitQuestCard };
                    }
                    break;
                case State.UsingExcalibur:
                    if (CurrentTeam.Excalibur.Holder == player)
                    {
                        return new List<Action.Type>() { Action.Type.UseExcalibur };
                    }
                    break;
                case State.LadyOfTheLake:
                    if (Rules.Contains(Rule.IncludeLadyOfTheLake) && CurrentRound.LadyOfTheLake != null && CurrentRound.LadyOfTheLake.Holder == player && CurrentRound.LadyOfTheLake.Target == null)
                    {
                        return new List<Action.Type>() { Action.Type.UseTheLadyOfTheLake };
                    }
                    break;
                case Game.State.GuessingMerlin:
                    if (player != null && player.Character == Character.Assassin)
                        return new List<Action.Type>() { Action.Type.GuessMerlin, Action.Type.Message };
                    return new List<Action.Type>() { Action.Type.Message };
            }
            return new List<Action.Type>();

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

        public void Message(Player player, string message)
        {
            CurrentRound.CurrentTeam.Messages.Add(new PlayerMessage { Player = player, Message = message });
        }        

        public Knowledge PlayerKnowledge(Player myself, Player someoneelse)
        {
            if (myself == null)
                return Knowledge.Player;

            //lancelots can know each other
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

            //minions know each other (except oberon)
            if (myself.Character == Character.Assassin || myself.Character == Character.Morgana || myself.Character == Character.MinionOfMordred || myself.Character == Character.Mordred)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morgana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Mordred || someoneelse.Character == Character.EvilLancelot)
                {
                    return Knowledge.Evil;
                }
            }

            //merlin knows minions (except mordred)
            if (myself.Character == Character.Merlin)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morgana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Oberon)
                {
                    return Knowledge.Evil;
                }
            }

            //percy knows merlin and morgana
            if (myself.Character == Character.Percival)
            {
                if (someoneelse.Character == Character.Merlin || someoneelse.Character == Character.Morgana)
                {
                    return Knowledge.Magical;
                }
            }

            return Knowledge.Player;
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

        public bool IsCharacterEvil(Character character, bool lancelotAllegianceSwitched)
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
                    if (lancelotAllegianceSwitched)
                    {
                        return true;
                    }
                    break;
                case Core.Character.EvilLancelot:
                    if (!lancelotAllegianceSwitched)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }


        public void AddToTeam(Player player, Player proposedPlayer)
        {
            if (CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new Exception("Hax. Player is already on the team");

            if (player != CurrentTeam.Leader)
                throw new Exception("Hax. Player is not the leader of this team");

            CurrentTeam.TeamMembers.Add(proposedPlayer);

            if (CurrentTeam.TeamMembers.Count == CurrentTeam.TeamSize)
            {
                if (Rules != null && Rules.Contains(Rule.IncludeExcalibur))
                {
                    GameState = State.AssigningExcalibur;
                }
                else
                {
                    GameState = State.VotingForTeam;
                }
            }
        }

        public void AssignExcalibur(Player player, Player proposedPlayer)
        {
            if (!Rules.Contains(Rule.IncludeExcalibur))
            {
                throw new Exception("Game does not include excalibur");
            }

            if (player != CurrentTeam.Leader)
                throw new Exception("Hax. Player is not the leader of this team");

            if (!CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is not on team..");

            if (proposedPlayer == CurrentTeam.Leader)
                throw new Exception("Leader cannot assign excalibur to themself");

            CurrentTeam.Excalibur.Holder = proposedPlayer;

            GameState = State.VotingForTeam;
        }

        public void UseExcalibur(Player player, Player proposedPlayer)
        {
            if (!Rules.Contains(Rule.IncludeExcalibur))
            {
                throw new Exception("Game does not include excalibur");
            }

            if (player != CurrentTeam.Excalibur.Holder)
                throw new Exception("Hax. Player does not have excalibur");

            if (proposedPlayer != null && !CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is not on team..");

            CurrentTeam.Excalibur.UsedOn = CurrentTeam.Quests.First(p => p.Player == proposedPlayer);
            CurrentTeam.Excalibur.OriginalMissionWasSuccess = CurrentTeam.Excalibur.UsedOn.Success;
            CurrentTeam.Excalibur.UsedOn.Success = !CurrentTeam.Excalibur.UsedOn.Success;

            OnTeamFinished();
        }

        private void OnTeamFinished()
        {
            if (Rules.Contains(Rule.IncludeLadyOfTheLake) && Rounds.Count >= 2)
            {
                GameState = State.LadyOfTheLake;
                return;
            }

            OnRoundFinished();
        }

        public void VoteForTeam(Player player, bool approve)
        {
            if (CurrentTeam.Votes.Any(v => v.Player == player))
                throw new Exception("Player has already voted");

            CurrentTeam.Votes.Add(new Vote { Approve = approve, Player = player });

            if (CurrentTeam.Votes.Count < GameSize)
                return;

            //on the last vote, if it fails, create the next quest
            var rejects = CurrentTeam.Votes.Count(v => !v.Approve);
            if (rejects >= Math.Ceiling(GameSize / 2.0))
            {
                if (CurrentRound.Teams.Count == 5)
                {
                    GameState = State.EternalChaos;
                }
                else
                {
                    CurrentRound.Teams.Add(new Team(Players.Next(CurrentTeam.Leader), CurrentRound.TeamSize, CurrentRound.RequiredFails));
                    GameState = State.ChoosingTeam;
                }
            }
            else
            {
                GameState = State.Questing;
            }
        }

        public void SubmitQuest(Player player, bool success)
        {
            if (Rules.Contains(Rule.GoodMustAlwaysVoteSucess) && !success && !IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
            {
                throw new Exception("Good must always vote success");
            }
            if (Rules.Contains(Rule.LancelotsMustVoteFanatically) && (player.Character == Character.Lancelot || player.Character == Character.EvilLancelot))
            {
                if ((success && IsCharacterEvil(player.Character, LancelotAllegianceSwitched)) || (!success && !IsCharacterEvil(player.Character, LancelotAllegianceSwitched)))
                {
                    throw new Exception("Lancelot must move fanatically");
                }
            }

            if (CurrentTeam.Quests.Select(v => v.Player).ToList().Contains(player))
                throw new Exception("Player has already submitted their quest card..");

            CurrentTeam.Quests.Add(new Quest { Player = player, Success = success });

            if (CurrentTeam.Quests.Count == CurrentTeam.TeamSize)
            {
                OnTeamFinished();
            }
        }

        public void UseLadyOfTheLake(Player player, Player target)
        {
            if (GameState != State.LadyOfTheLake)
                throw new Exception("wrong state for this action");

            if (CurrentRound.LadyOfTheLake != null && CurrentRound.LadyOfTheLake.Holder != player)
                throw new Exception("Hax. Player does not have lady of the lake.");

            CurrentRound.LadyOfTheLake.Target = target;
            CurrentRound.LadyOfTheLake.IsEvil = IsCharacterEvil(target.Character, LancelotAllegianceSwitched);

            OnRoundFinished();
        }

    }
}
