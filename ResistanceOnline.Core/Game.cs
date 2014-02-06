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
            Rounds,
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
            TotalPlayers = players;
        }

        public List<Character> AvailableCharacters { get; set; }
        public int TotalPlayers { get; set; }
        public List<Player> Players { get; set; }
        public List<Round> Rounds { get; set; }
        public int QuestIndicator { get; set; }

        public void AddCharacter(Character character)
        {
            if (AvailableCharacters.Count == TotalPlayers)
                throw new Exception("All roles added");
            AvailableCharacters.Add(character);
        }

        public void JoinGame(string playerName)
        {
            if (Players.Count == TotalPlayers)
                throw new Exception("Game already full");

            if (Players.Select(p=>p.Name).Contains(playerName))
                throw new Exception("Player name already taken");

            Players.Add(new Player() { Name = playerName });

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
                Rounds.Add(new Round(Players, random.Next(TotalPlayers)));
            }                
        }

        private Round CurrentRound { get { return Rounds.Last(); } }

        public void ProposePlayer(Player proposedPlayer)
        {
            CurrentRound.ProposePlayer(proposedPlayer);
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
                Rounds.Add(new Round(Players, CurrentRound.NextPlayer));
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
                //todo - merlin/assassin
                return State.GoodPrevails;
            }
            
            return State.Rounds;
        }
    }
}
