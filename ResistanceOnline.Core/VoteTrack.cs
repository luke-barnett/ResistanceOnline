using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// A quest that get's voted on then may go ahead
    /// </summary>
    public class VoteTrack
    {
        public Player Leader { get; set; }
        public ExcaliburUse Excalibur { get; set; }
        public List<Player> Players { get; set; }
        public List<VoteToken> Votes { get; set; }
        public List<QuestCard> QuestCards { get; set; }
        public List<PlayerMessage> Messages { get; set; }
        public int QuestSize { get; set; }
        public int RequiredFails { get; set; }

        public bool? IsSuccess
        {
            get
            {
                if (QuestCards.Count < QuestSize)
                    return null;
                return QuestCards.Count(c => !c.Success) < RequiredFails;
            }
        }       

        public VoteTrack(Player leader, int size, int requiredFails)
        {
            Leader = leader;
            QuestSize = size;
            RequiredFails = requiredFails;
            Players = new List<Player>();
            Votes = new List<VoteToken>();
            QuestCards = new List<QuestCard>();
            Messages = new List<PlayerMessage>();
        }

    }
}

