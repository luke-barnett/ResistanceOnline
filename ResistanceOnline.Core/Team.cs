using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// A quest that get's voted on then may go ahead
    /// </summary>
    public class Team
    {
        public Player Leader { get; set; }
        public ExcaliburUse Excalibur { get; set; }
        public List<Player> TeamMembers { get; set; }
        public List<Vote> Votes { get; set; }
        public List<Quest> Quests { get; set; }
        public List<PlayerMessage> Messages { get; set; }
        public int TeamSize { get; set; }
        public int RequiredFails { get; set; }

        public bool? IsSuccess
        {
            get
            {
                if (Quests.Count < TeamSize)
                    return null;
                return Quests.Count(c => !c.Success) < RequiredFails;
            }
        }       

        public Team(Player leader, int size, int requiredFails)
        {
            Leader = leader;
            TeamSize = size;
            RequiredFails = requiredFails;
            TeamMembers = new List<Player>();
            Votes = new List<Vote>();
            Quests = new List<Quest>();
            Messages = new List<PlayerMessage>();
        }

    }
}

