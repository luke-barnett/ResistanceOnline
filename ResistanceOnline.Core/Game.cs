using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// the setup of the game, players, characters etc
    /// </summary>
    public class Game
    {
        public enum State
        {
            Setup,
            Playing,
            Finished
        }

        public int GameId { get; set; }
        public State GameState { get; set; }
        public List<Character> AvailableCharacters { get; set; }
        public List<Player> Players { get; set; }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }
        public List<Rule> Rules { get; set; }
        public Player InitialHolderOfLadyOfTheLake { get; set; }
        public Player InitialLeader { get; set; }
        public List<QuestSize> RoundTables;

        public Game()
        {
            Players = new List<Player>();
            AvailableCharacters = new List<Character>();
            LoyaltyDeck = new List<LoyaltyCard> { LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.SwitchAlegiance, LoyaltyCard.SwitchAlegiance }.Shuffle().ToList();
            Rules = new List<Rule>() { Rule.LadyOfTheLakeExists };
            RoundTables = StandardRoundTables(0);
            GameState = State.Setup;
        }

		public Game(ResistanceOnline.Database.Entities.Game game)
		{
			GameId = game.GameId;
            if (game.GameState != null)
            {
                GameState = (State)Enum.Parse(typeof(State), game.GameState);
            }            
			AvailableCharacters = game.Characters.Select(x=>(Character)Enum.Parse(typeof(Character), x.Name)).ToList();
			Players = game.Players.Select(x=>new Player(x)).ToList();
			LoyaltyDeck = game.LoyaltyDeck.Select(x=>(LoyaltyCard)Enum.Parse(typeof(LoyaltyCard), x.Card)).ToList();
			Rules = game.Rules.Select(x=>(Rule)Enum.Parse(typeof(Rule), x.Name)).ToList();
            RoundTables = game.RoundTables.Select(x => new QuestSize(x)).ToList();

            if (Players != null && Players.Count > 0)
            {
                InitialHolderOfLadyOfTheLake = Players.First(p => p.Name == game.InitialHolderOfLadyOfTheLake);
                InitialLeader = Players.First(p => p.Name == game.InitialLeader);
            }
		}

        public Guid JoinGame(string playerName, Guid playerGuid, Player.Type playerType=Player.Type.Human)
        {
            if (Players.Select(p => p.Guid).Contains(playerGuid))
                throw new InvalidOperationException("It's really not fair if you play as more than one player and you want the game to be fair don't you?");

            if (String.IsNullOrWhiteSpace(playerName))
                playerName = String.Empty;

            playerName = playerName.Uniquify(Players.Select(p => p.Name));

            Players.Add(new Player() { Name = playerName, Guid = playerGuid, PlayerType = playerType });

            var evilCount = AvailableCharacters.Count(c => IsCharacterEvil(c, false));
            if (evilCount < (AvailableCharacters.Count / 3.0))
            {
                AvailableCharacters.Add(Character.MinionOfMordred);
            }
            else
            {
                AvailableCharacters.Add(Character.LoyalServantOfArthur);
            }

            RoundTables = StandardRoundTables(Players.Count);

            return playerGuid;
        }

        public bool ContainsLancelot()
        {
            return (AvailableCharacters.Contains(Character.EvilLancelot) || AvailableCharacters.Contains(Character.Lancelot));
        }

        public LoyaltyCard? GetLoyaltyCard(int roundNumber)
        {
            if (Rules.Contains(Rule.LoyaltyCardsAreDeltInAdvance))
                return null;
            if (!ContainsLancelot())
                return null;
            return LoyaltyDeck[roundNumber - 1];
        }

        public void AllocateCharacters()
        {
            var characterCards = AvailableCharacters.ToList();
            Random random = new Random();
            foreach (var player in Players)
            {
                var index = random.Next(characterCards.Count);
                player.Character = characterCards[index];
                characterCards.RemoveAt(index);
            }
        }

        public void ChooseLeader()
        {
            InitialHolderOfLadyOfTheLake = Players.Random();
            InitialLeader = Players.Next(InitialHolderOfLadyOfTheLake);
        }


        public List<QuestSize> StandardRoundTables(int GameSize)
        {
            var roundTables = new List<QuestSize>();
            if (GameSize <= 5)
            {
                roundTables.Add(new QuestSize(2));
                roundTables.Add(new QuestSize(3));
                roundTables.Add(new QuestSize(2));
                roundTables.Add(new QuestSize(3));
                roundTables.Add(new QuestSize(3));
                return roundTables;
            }

            switch (GameSize)
            {
                case 6:
                    roundTables.Add(new QuestSize(2));
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    break;
                case 7:
                    roundTables.Add(new QuestSize(2));
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4, 2));
                    roundTables.Add(new QuestSize(4));
                    break;
                case 8:
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(5, 2));
                    roundTables.Add(new QuestSize(5));
                    break;
                case 9:
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(5, 2));
                    roundTables.Add(new QuestSize(5));
                    break;
                default:
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(5, 2));
                    roundTables.Add(new QuestSize(5));
                    break;
            }
            return roundTables;
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

        public void StartGame()
        {
            AllocateCharacters();
            ChooseLeader();
            GameState = State.Playing;
        }

    }
}
