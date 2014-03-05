using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class GameSetup
    {
        public List<Character> AvailableCharacters { get; set; }
        public List<Player> Players { get; set; }
        public int GameSize { get { return Players.Count; } }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }
        public List<Rule> Rules { get; set; }
        public int GameId { get; set; }
        public Player InitialHolderOfLadyOfTheLake { get; set; }
        public Player InitialLeader { get; set; }
        public List<RoundTable> RoundTables;
        public bool GameStarted { get; set; }

        public GameSetup()
        {
            Players = new List<Player>();
            AvailableCharacters = new List<Character>();
            LoyaltyDeck = new List<LoyaltyCard> { LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.SwitchAlegiance, LoyaltyCard.SwitchAlegiance }.Shuffle().ToList();
            Rules = new List<Rule>() { Rule.IncludeLadyOfTheLake };
            RoundTables = StandardRoundTables(0);
        }

        public void SetCharacter(int index, Character character)
        {
            AvailableCharacters[index] = character;
        }

        public Guid JoinGame(string playerName, Guid playerGuid)
        {
            if (String.IsNullOrWhiteSpace(playerName))
                playerName = String.Empty;
            playerName = playerName.Uniquify(Players.Select(p => p.Name));

            Players.Add(new Player() { Name = playerName, Guid = playerGuid });

            var evilCount = AvailableCharacters.Count(c => IsCharacterEvil(c, false));
            if (evilCount < (AvailableCharacters.Count / 3.0))
            {
                AvailableCharacters.Add(Character.MinionOfMordred);
            }
            else
            {
                AvailableCharacters.Add(Character.LoyalServantOfArthur);
            }

            RoundTables = StandardRoundTables(GameSize);

            return playerGuid;
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


        public List<RoundTable> StandardRoundTables(int GameSize)
        {
            var roundTables = new List<RoundTable>();
            if (GameSize <= 5)
            {
                roundTables.Add(new RoundTable(2));
                roundTables.Add(new RoundTable(3));
                roundTables.Add(new RoundTable(2));
                roundTables.Add(new RoundTable(3));
                roundTables.Add(new RoundTable(3));
                return roundTables;
            }

            switch (GameSize)
            {
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
                default:
                    roundTables.Add(new RoundTable(3));
                    roundTables.Add(new RoundTable(4));
                    roundTables.Add(new RoundTable(4));
                    roundTables.Add(new RoundTable(5, 2));
                    roundTables.Add(new RoundTable(5));
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


    }
}
