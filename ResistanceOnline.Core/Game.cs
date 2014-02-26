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
            Playing,
            GuessingMerlin,
            Finished,
        }

        public State GameState { get; set; }
        public int GameId { get; set; }
        public List<Character> AvailableCharacters { get; set; }
        public List<PlayerMessage> Messages { get; set; }
        public List<Player> Players { get; set; }
        public List<Round> Rounds { get; set; }
        public int QuestIndicator { get; set; }
        public List<MerlinGuess> MerlinGuesses { get; set; }
        public Player HolderOfLadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }
        public List<Rule> Rules { get; set; }
        public int GameSize { get { return Players.Count; } }

        public Game()
        {
            GameState = State.Setup;
            Players = new List<Player>();
            Rounds = new List<Round>();
            AvailableCharacters = new List<Character>();
            LoyaltyDeck = new List<LoyaltyCard> { LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.SwitchAlegiance, LoyaltyCard.SwitchAlegiance }.Shuffle().ToList();
            MerlinGuesses = new List<MerlinGuess>();

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

        public void UseLadyOfTheLake(Player player, Player target)
        {
            HolderOfLadyOfTheLake = target;
            CurrentRound.UseLadyOfTheLake(player, target);
        }

        public void GuessMerlin(Player player, Player guess)
        {
            if (GameState != State.GuessingMerlin)
            {
                throw new Exception("Hax. You shouldn't be guessing merlin at this stage");
            }

            var merlinGuess = MerlinGuesses.FirstOrDefault(g => g.Assassin == player);

            if (merlinGuess == null)
            {
                throw new Exception("Hax. Player is not assassin.");
            }

            if (merlinGuess.Guess != null)
                throw new Exception("Hax. Assassin has already guessed.");

            merlinGuess.Guess = guess;
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

            AllocateCharactersToPlayers();            
        }

        public Guid JoinGame(string playerName, Guid playerGuid)
        {
            playerName = playerName.Uniquify(Players.Select(p => p.Name));

            Players.Add(new Player() { Name = playerName, Guid = playerGuid });

            var evilCount = AvailableCharacters.Count(c=> IsCharacterEvil(c));
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

        private void AllocateCharactersToPlayers()
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

            GameState = State.Playing;
            var HolderOfLadyOfTheLake = Players.Random();
            var leader = Players.Next(HolderOfLadyOfTheLake);

            //create first round
            NextRound(leader);
        }

        public Round CurrentRound { get { return Rounds.LastOrDefault(r=>r.RoundState != Round.State.Finished); } }

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

            CurrentRound.UseExcalibur(player, proposedPlayer);
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
        }

        private void NextRound(Player leader)
        {
            var roundTable = RoundTables[Rounds.Count];
            var round = new Round(Players, leader, HolderOfLadyOfTheLake, roundTable.TeamSize, roundTable.RequiredFails, Rules, LancelotAllegianceSwitched);
            round.Finished += Round_Finished;
            Rounds.Add(round);
        }

        void Round_Finished(object sender, EventArgs e)
        {
            //3 failed missions, don't bother going any further
            if (Rounds.Count(r => !r.IsSuccess.Value) >= 3)
            {
                GameState = State.Finished;
                return;
            }

            //3 successful missions, don't bother going any further
            if (Rounds.Count(r => r.IsSuccess.Value) >= 3)
            {
                if (MerlinGuesses.Count > 0)
                {
                    GameState = State.GuessingMerlin;
                }
                else
                {
                    GameState = State.Finished;
                }
                return;
            }

            //loyalty cards            
            var loyaltyCard = GetLoyaltyCard(Rounds.Count);
            if (loyaltyCard == LoyaltyCard.SwitchAlegiance)
            {
                LancelotAllegianceSwitched = !LancelotAllegianceSwitched;
            }
        }

        public List<Action.Type> AvailableActions(Player player)
        {
            switch (GameState)
            {
                case Game.State.Playing:
                    return CurrentRound.AvailableActions(player);

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

        public bool IsCharacterEvil(Character character)
        {
            switch (character)
            {
                case Core.Character.Assassin:
                case Core.Character.MinionOfMordred:
                case Core.Character.Mordred:
                case Core.Character.Morgana:
                case Core.Character.Oberon:
                case Core.Character.EvilLancelot:
                    return true;
            }
            return false;
        }
    }
}
