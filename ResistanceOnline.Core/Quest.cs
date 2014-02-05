using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Quest
    {
        public enum State
        {
            Unstarted,            
            Voting,
            Questing,
            Succeeded,
            Failed,
            FailedAllVotes
        }

        public int Size { get; set; }
        public int RequiredFails { get; set; }

        public List<Player> PlayersOnQuest { get; set; }
        public List<QuestCard> PlayedQuestCards { get; set; }        

        public int VoteTrackIndicator { get; set; }


    }
}
