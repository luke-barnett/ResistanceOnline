using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Game
    {
        public enum State
        {
            GameSetup,
            Playing,
            EvilTriumphs,
            GoodPrevails,
            GuessingMerlin,
            MerlinDies
        }

        public Game(int players)
        {
            Players = new List<Player>();
            Rounds = new List<Round>();
            AvailableCharacters = new List<Character>();
            GameSize = players;
            LadyOfTheLakeUses = new List<LadyOfTheLakeUse>();

            RoundTables = new List<RoundTable>();
            switch (players)
            {
                case 5:
                    RoundTables.Add(new RoundTable(2));
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(2));
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(3));
                    break;
                case 6:
                    RoundTables.Add(new RoundTable(2));
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(4));
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(4));
                    break;
                case 7:
                    RoundTables.Add(new RoundTable(2));
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(4,2));
                    RoundTables.Add(new RoundTable(4));
                    break;
                case 8:
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(4));
                    RoundTables.Add(new RoundTable(4));
                    RoundTables.Add(new RoundTable(5,2));
                    RoundTables.Add(new RoundTable(5));
                    break;
                case 9:
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(4));
                    RoundTables.Add(new RoundTable(4));
                    RoundTables.Add(new RoundTable(5,2));
                    RoundTables.Add(new RoundTable(5));
                    break;
                case 10:
                    RoundTables.Add(new RoundTable(3));
                    RoundTables.Add(new RoundTable(4));
                    RoundTables.Add(new RoundTable(4));
                    RoundTables.Add(new RoundTable(5,2));
                    RoundTables.Add(new RoundTable(5));
                    break;
                default:
                    throw new Exception("No tableaus for games with " + players + " players");
            }
        }

        public int GameId { get; set; }
        public List<Character> AvailableCharacters { get; set; }
        public int GameSize { get; set; }
        public List<Player> Players { get; set; }
        public List<Round> Rounds { get; set; }
        public int QuestIndicator { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }
        public List<RoundTable> RoundTables { get; set; }
        public List<LadyOfTheLakeUse> LadyOfTheLakeUses { get; set; }
        public Player HolderOfLadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }

        public bool Rule_PlayersCanImpersonateOtherPlayers { get; set; }
        public bool Rule_IncludeLadyOfTheLake { get; set; }
        public bool Rule_LancelotsKnowEachOther { get; set; }
        public bool Rule_GoodMustAlwaysVoteSucess { get; set; }

        public void UseLadyOfTheLake(Player player, Player target)
        {
            if (HolderOfLadyOfTheLake != player)
                throw new Exception("Hax. Player does not have lady of the lake.");

            LadyOfTheLakeUses.Add(new LadyOfTheLakeUse { UsedBy = player, UsedOn = target });
            
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
            foreach (Bots.IBot bot in Players.Where(p => p is Bots.IBot))
            {
                bot.DoSomething(this);
            }
                
        }

        public void AddCharacter(Character character)
        {
            if (AvailableCharacters.Count == GameSize)
                throw new Exception("All roles added");
            AvailableCharacters.Add(character);

            OnCharacterAddedOrPlayerJoined();
        }


        private void OnCharacterAddedOrPlayerJoined()
        {
            //check if game ready to start
            if (AvailableCharacters.Count == GameSize && Players.Count == GameSize)
            {
                OnAllCharactersAndPlayersAdded();
            }
        }

        private void OnAllCharactersAndPlayersAdded()
        {
            AllocateCharactersToPlayers();
        }

        public Guid JoinGame(string playerName)
        {
            if (Players.Count == GameSize)
                throw new Exception("Game already full");

            if (Players.Select(p=>p.Name).Contains(playerName))
                throw new Exception("Player name already taken");

            var guid = Guid.NewGuid();
            Players.Add(new Player() { Name = playerName, Guid = guid });

            OnCharacterAddedOrPlayerJoined();

            OnAfterAction();

            return guid;
        }

        public Guid AddSimpleBot(string name)
        {
            if (Players.Count == GameSize)
                throw new Exception("Game already full");

            if (Players.Select(p => p.Name).Contains(name))
                throw new Exception("Player name already taken");

            var guid = Guid.NewGuid();
            Players.Add(new Bots.SimpleBot() { Name = name, Guid = guid });

            OnCharacterAddedOrPlayerJoined();

            return guid;
        }


        private void AllocateCharactersToPlayers()
        {
            //on last player, allocate characters
            if (Players.Count == GameSize)
            {
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
        }

        private void OnGameStart()
        {
            //create first round
            var leader = new Random().Next(GameSize);
            if (Rule_IncludeLadyOfTheLake)
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

        public Round CurrentRound { get { return Rounds.Last(); } }

        public void AddToTeam(Player player, Player proposedPlayer)
        {
            CurrentRound.AddToTeam(player, proposedPlayer);
        }

        public void VoteForTeam(Player player, bool approve)
        {
            CurrentRound.VoteForTeam(player, approve);           
        }

        public void SubmitQuest(Player player, bool success)
        {
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

            if (roundNumber >= 2 && Rule_IncludeLadyOfTheLake)
            {
                //wait for lady of the lake to be used
                return;
            }

            OnStartNextRound();
        }

        private void OnLadyOfTheLakeUsed()
        {
            HolderOfLadyOfTheLake = LadyOfTheLakeUses.Last().UsedOn;
            OnStartNextRound();
        }

        private void OnStartNextRound()
        {
            //create the next round
            CreateRound(CurrentRound.NextPlayer);            
        }


        public State DetermineState()
        {
            if (AvailableCharacters.Count < GameSize || Players.Count < GameSize)
                return State.GameSetup;

            if (Rounds.Where(r => r.DetermineState() == Round.State.Failed).Count() >= 3)
                return State.EvilTriumphs;

            if (Rounds.Where(r => r.DetermineState() == Round.State.Succeeded).Count() >= 3)
            {
                if (Rule_IncludeLadyOfTheLake && LadyOfTheLakeUses.Count < Rounds.Count - 2)
                    return State.Playing;

                if (AssassinsGuessAtMerlin == null && Players.Any(p=>p.Character == Character.Merlin))
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
                            if (player!=null && quest.Leader.Name == player.Name)
                            {
                                return new List<Action.Type>() { Action.Type.AddToTeam };
                            }
                            return new List<Action.Type>();
                        case Round.State.Voting:
                            if (player!=null && !quest.Votes.Select(v => v.Player.Name).ToList().Contains(player.Name))
                            {
                                return new List<Action.Type>() { Action.Type.VoteForTeam };
                            }
                            return new List<Action.Type>();
                        case Round.State.Questing:
                            if (player != null && quest.TeamMembers.Select(v => v.Name).ToList().Contains(player.Name) &&
                                !quest.Quests.Select(q => q.Player.Name).ToList().Contains(player.Name))
                            {
                                return new List<Action.Type>() { Action.Type.SubmitQuestCard };
                            }
                            return new List<Action.Type>();
                    }

                    //round over but still current
                    if (Rule_IncludeLadyOfTheLake && Rounds.Count >= 2 && HolderOfLadyOfTheLake == player)
                    {
                        return new List<Action.Type>() { Action.Type.UseTheLadyOfTheLake };
                    }

                    return new List<Action.Type>();
                
                case Game.State.GameSetup:
                    var actions = new List<Action.Type>();
                    if (player == null && Players.Count < GameSize)
                    {
                        actions.Add(Action.Type.JoinGame);
                    }
                    if (player != null && AvailableCharacters.Count < GameSize)
                    {
                        actions.Add(Action.Type.AddCharacter);
                    }
                    return actions;
                
                case Game.State.GuessingMerlin:
                    if (player != null && player.Character == Character.Assassin)
                        return new List<Action.Type>() { Action.Type.GuessMerlin };
                    return new List<Action.Type>();

                case Game.State.EvilTriumphs:
                case Game.State.GoodPrevails:
                case Game.State.MerlinDies:
                    return new List<Action.Type>();
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
                case Action.Type.AddCharacter:
                    AddCharacter(action.Character);
                    break;
                case Action.Type.GuessMerlin:
                    GuessMerlin(player, action.Player);
                    break;
                case Action.Type.JoinGame:
                    JoinGame(action.Name);
                    break;
                case Action.Type.AddToTeam:
                    AddToTeam(player, action.Player);
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
                default:
                    throw new NotImplementedException();
            }

            OnAfterAction();
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

            if (Rule_LancelotsKnowEachOther)
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

            if (DetectEvil(myself, someoneelse)) {
                return Knowledge.Evil;
            }

            if (DetectMerlin(myself, someoneelse))
            {
                return Knowledge.Merlin;
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
        public static bool DetectMerlin(Player myself, Player someoneelse)
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


    }
}
