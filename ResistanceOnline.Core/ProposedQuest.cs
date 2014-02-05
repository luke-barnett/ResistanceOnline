using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    public class ProposedQuest
    {
        public enum State
        {
            NotYet,
            WaitingForLeader,
            WaitingForVotes,
            Accepted,
            Rejected
        }

        public Player Leader { get; set; }
        public int Size { get; set; }
        public List<Player> ProposedPlayers { get; set; }
        public List<QuestVote> Votes { get; set; }
    }
}
