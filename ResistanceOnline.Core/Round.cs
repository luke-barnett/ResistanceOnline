using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// A set of up to 5 quests that get proposed and voted on
    /// </summary>
    public class Round
    {
        public enum State
        {
            Unstarted,            
            ProposingPlayers,
            Voting,
            Questing,
            Succeeded,
            Failed,
            FailedAllVotes
        }

        List<Player> _players;
        int _currentPlayer;

        public Round(List<Player> players, int currentPlayer, int size, int requiredFails)
        {
            TotalPlayers = players.Count;
            Size = size;
            RequiredFails = requiredFails;

            Quests = new List<Quest>();
            Quests.Add(new Quest(players[currentPlayer]));

            _players = players;
            _currentPlayer = currentPlayer;
        }

        public int TotalPlayers { get; set; }
        public int Size { get; set; }
        public int RequiredFails { get; set; }
        public int NextPlayer { get { return (_currentPlayer + 1) % TotalPlayers; } }

        public Quest CurrentQuest { get { return Quests.Last(); } }

        public List<Quest> Quests { get; set; }

        public void ProposePlayer(Player proposedPlayer)
        {
            CurrentQuest.ProposePlayer(proposedPlayer);
        }

        public void VoteForQuest(Player player, bool approve)
        {
            CurrentQuest.VoteForQuest(player, approve);

            //on the last vote, if it fails, create the next quest
            if (CurrentQuest.Votes.Count == TotalPlayers)
            {
                var rejects = CurrentQuest.Votes.Where(v => !v.Approve).Count();
                if (rejects >= Math.Ceiling(TotalPlayers / 2.0))
                {
                    _currentPlayer = NextPlayer;
                    Quests.Add(new Quest(_players[_currentPlayer]));
                }
            }
        }

        public void SubmitQuest(Player player, bool success)
        {
            CurrentQuest.SubmitQuest(player, success);
        }

        public State DetermineState()
        {
            //no more than 5 quest votes per round
            if (Quests.Count > 5)
                return State.FailedAllVotes;
            
            //proposing
            if (CurrentQuest.ProposedPlayers.Count < Size)
                return State.ProposingPlayers;

            //voting on proposing
            if (CurrentQuest.Votes.Count < TotalPlayers)
                return State.Voting;

            //questing
            if (CurrentQuest.QuestCards.Count < Size)
                return State.Questing;

            //finished
            var fails = CurrentQuest.QuestCards.Where(c => !c.Success).Count();
            if (fails >= RequiredFails)
                return State.Failed;

            return State.Succeeded;
           
        }

    }
}
