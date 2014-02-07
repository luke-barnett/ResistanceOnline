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
            WaitingForCharacterSetup,
            WaitingForPlayers,
            InPlay,
            EvilTriumphs,
            GoodPrevails,
            GuessingMerlin,
            MerlinDies
        }

        public Game(int players, bool impersonationEnabled)
        {
            ImpersonationEnabled = impersonationEnabled;
            Players = new List<Player>();
            Rounds = new List<Round>();
            AvailableCharacters = new List<Character>();
            TotalPlayers = players;
        }

        private class TableausRound
        {
            public int Round { get; set; }
            public int Players { get; set; }
            public int QuestSize { get; set; }
            public int RequiredFails { get; set; }
        }

        List<TableausRound> TablausRoundDefinitions = new List<TableausRound>()
        {
            new TableausRound{ Players = 5, Round = 1, QuestSize=2, RequiredFails = 1 },
            new TableausRound{ Players = 5, Round = 2, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 5, Round = 3, QuestSize=2, RequiredFails = 1 },
            new TableausRound{ Players = 5, Round = 4, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 5, Round = 5, QuestSize=3, RequiredFails = 1 },

            new TableausRound{ Players = 6, Round = 1, QuestSize=2, RequiredFails = 1 },
            new TableausRound{ Players = 6, Round = 2, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 6, Round = 3, QuestSize=4, RequiredFails = 1 },
            new TableausRound{ Players = 6, Round = 4, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 6, Round = 5, QuestSize=4, RequiredFails = 1 },

            new TableausRound{ Players = 7, Round = 1, QuestSize=2, RequiredFails = 1 },
            new TableausRound{ Players = 7, Round = 2, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 7, Round = 3, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 7, Round = 4, QuestSize=4, RequiredFails = 2 },
            new TableausRound{ Players = 7, Round = 5, QuestSize=4, RequiredFails = 1 },

            new TableausRound{ Players = 8, Round = 1, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 8, Round = 2, QuestSize=4, RequiredFails = 1 },
            new TableausRound{ Players = 8, Round = 3, QuestSize=4, RequiredFails = 1 },
            new TableausRound{ Players = 8, Round = 4, QuestSize=5, RequiredFails = 2 },
            new TableausRound{ Players = 8, Round = 5, QuestSize=5, RequiredFails = 1 },

            new TableausRound{ Players = 9, Round = 1, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 9, Round = 2, QuestSize=4, RequiredFails = 1 },
            new TableausRound{ Players = 9, Round = 3, QuestSize=4, RequiredFails = 1 },
            new TableausRound{ Players = 9, Round = 4, QuestSize=5, RequiredFails = 2 },
            new TableausRound{ Players = 9, Round = 5, QuestSize=5, RequiredFails = 1 },

            new TableausRound{ Players = 10, Round = 1, QuestSize=3, RequiredFails = 1 },
            new TableausRound{ Players = 10, Round = 2, QuestSize=4, RequiredFails = 1 },
            new TableausRound{ Players = 10, Round = 3, QuestSize=4, RequiredFails = 1 },
            new TableausRound{ Players = 10, Round = 4, QuestSize=5, RequiredFails = 2 },
            new TableausRound{ Players = 10, Round = 5, QuestSize=5, RequiredFails = 1 },

        };

        public List<Character> AvailableCharacters { get; set; }
        public int TotalPlayers { get; set; }
        public bool ImpersonationEnabled { get; set; }
        public List<Player> Players { get; set; }
        public List<Round> Rounds { get; set; }
        public int QuestIndicator { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }

        public void GuessMerlin(Player player, Player guess)
        {
            if (player.Character != Character.Assassin)
                throw new Exception("Hax. Player is not assassin.");

            if (AssassinsGuessAtMerlin != null)
                throw new Exception("Hax. Assassin has already guessed.");

            AssassinsGuessAtMerlin = guess;
        }

        public void AddCharacter(Character character)
        {
            if (AvailableCharacters.Count == TotalPlayers)
                throw new Exception("All roles added");
            AvailableCharacters.Add(character);

            //on last character, allocate characters 
            if (AvailableCharacters.Count == TotalPlayers && Players.Count == TotalPlayers)
            {
                Allocate();
            }
        }

        public Guid JoinGame(string playerName)
        {
            if (Players.Count == TotalPlayers)
                throw new Exception("Game already full");

            if (Players.Select(p=>p.Name).Contains(playerName))
                throw new Exception("Player name already taken");

            var guid = Guid.NewGuid();
            Players.Add(new Player() { Name = playerName, Guid = guid });

            //on last player, allocate characters if 
            if (AvailableCharacters.Count == TotalPlayers)
            {
                Allocate();
            }
            return guid;
        }

        private void Allocate()
        {
            //on last player, allocate characters
            if (Players.Count == TotalPlayers)
            {
                if (AvailableCharacters.Count != TotalPlayers)
                    throw new Exception("Not Enough Characters for Players");

                var characterCards = AvailableCharacters.ToList();
                Random random = new Random();
                foreach (var player in Players)
                {
                    var index = random.Next(characterCards.Count);
                    player.Character = characterCards[index];
                    characterCards.RemoveAt(index);
                }

                //create first round
                CreateRound(random.Next(TotalPlayers));
            }


        }

        private void CreateRound(int leader)
        {
            var tablaus = TablausRoundDefinitions.FirstOrDefault(t => t.Players == TotalPlayers && t.Round == 1);
            if (tablaus == null)
                throw new Exception("Missing definitions for games with " + TotalPlayers + " players in round " + 1);
            Rounds.Add(new Round(Players, leader, tablaus.QuestSize, tablaus.RequiredFails));

        }

        public Round CurrentRound { get { return Rounds.Last(); } }

        public void PutOnQuest(Player player, Player proposedPlayer)
        {
            CurrentRound.PutOnQuest(player, proposedPlayer);
        }

        public void VoteForQuest(Player player, bool approve)
        {
            CurrentRound.VoteForQuest(player, approve);           
        }

        public void SubmitQuest(Player player, bool success)
        {
            CurrentRound.SubmitQuest(player, success);

            //on last quest submit, create the next round
            var roundState = CurrentRound.DetermineState();
            if (roundState == Round.State.Succeeded || roundState == Round.State.Failed)
            {
                //3 failed missions, don't bother going any further
                if (Rounds.Select(r => r.DetermineState() == Round.State.Failed).Count() >= 3)
                    return;

                //3 successful missions, don't bother going any further
                if (Rounds.Select(r => r.DetermineState() == Round.State.Succeeded).Count() >= 3)
                    return;

                //create the next round
                CreateRound(CurrentRound.NextPlayer);
            }

        }

        public State DetermineState()
        {
            if (AvailableCharacters.Count < TotalPlayers)
                return State.WaitingForCharacterSetup;

            if (Players.Count < TotalPlayers)
                return State.WaitingForPlayers;

            if (Rounds.Select(r => r.DetermineState() == Round.State.Failed).Count() >= 3)
                return State.EvilTriumphs;

            if (Rounds.Select(r => r.DetermineState() == Round.State.Succeeded).Count() >= 3)
            {
                if (AssassinsGuessAtMerlin == null)
                    return State.GuessingMerlin;

                if (AssassinsGuessAtMerlin.Character == Character.Merlin)
                    return State.MerlinDies;

                return State.GoodPrevails;
            }

            return State.InPlay;
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
                case Game.State.InPlay:
                    var roundState = CurrentRound.DetermineState();
                    var quest = CurrentRound.CurrentQuest;
                    switch (roundState)
                    {
                        case Round.State.ProposingPlayers:
                            if (quest.Leader.Name == player.Name)
                            {
                                return new List<Action.Type>() { Action.Type.PutOnQuest };
                            }
                            return new List<Action.Type>();
                        case Round.State.Voting:
                            if (!quest.Votes.Select(v => v.Player.Name).ToList().Contains(player.Name))
                            {
                                return new List<Action.Type>() { Action.Type.VoteForQuest };
                            }
                            return new List<Action.Type>();
                        case Round.State.Questing:
                            if (quest.ProposedPlayers.Select(v => v.Name).ToList().Contains(player.Name) &&
                                !quest.QuestCards.Select(q => q.Player.Name).ToList().Contains(player.Name))
                            {
                                return new List<Action.Type>() { Action.Type.SubmitQuestCard };
                            }
                            return new List<Action.Type>();
                    }

                    return new List<Action.Type>();
                case Game.State.WaitingForCharacterSetup:
                    if (player == null)
                    {
                        if (Players.Count == TotalPlayers)
                            return new List<Action.Type>();
                        return new List<Action.Type>() { Action.Type.JoinGame };
                    }
                    if (Players.Count == TotalPlayers)
                        return new List<Action.Type>() { Action.Type.AddCharacterCard };
                    return new List<Action.Type>() { Action.Type.JoinGame, Action.Type.AddCharacterCard };
                case Game.State.WaitingForPlayers:
                    if (player == null)
                    {
                        return new List<Action.Type>() { Action.Type.JoinGame };
                    }
                    else
                    {
                        return new List<Action.Type>();
                    }

                case Game.State.GuessingMerlin:
                    if (player.Character == Character.Assassin)
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
                case Action.Type.AddCharacterCard:
                    AddCharacter(action.Character);
                    break;
                case Action.Type.GuessMerlin:
                    GuessMerlin(player, action.Player);
                    break;
                case Action.Type.JoinGame:
                    JoinGame(action.Name);
                    break;
                case Action.Type.PutOnQuest:
                    PutOnQuest(player, action.Player);
                    break;
                case Action.Type.SubmitQuestCard:
                    SubmitQuest(player, action.Success);
                    break;
                case Action.Type.VoteForQuest:
                    VoteForQuest(player, action.Accept);
                    break;
                default:
                    throw new NotImplementedException();
            }
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
            if (myself.Character == Character.Assassin || myself.Character == Character.Morcana || myself.Character == Character.MinionOfMordred || myself.Character == Character.Mordred)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morcana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Mordred)
                {
                    return true;
                }
            }

            //merlin knows minions (except mordred)
            if (myself.Character == Character.Merlin)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morcana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Oberon)
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
                if (someoneelse.Character == Character.Merlin || someoneelse.Character == Character.Morcana)
                {
                    return true;
                }
            }

            return false;
        }

        public int GameId { get; set; }
    }
}
