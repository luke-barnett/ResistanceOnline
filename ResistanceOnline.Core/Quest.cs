using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Quest
    {
        public int QuestSize { get; set; }
        public int RequiredFails { get; set; }
        public List<VoteTrack> VoteTracks { get; set; }
        public VoteTrack CurrentVoteTrack { get { return VoteTracks.Last(); } }
        public LadyOfTheLakeUse LadyOfTheLake { get; set; }

        public bool? IsSuccess
        {
            get
            {
                return CurrentVoteTrack.IsSuccess;
            }
        }   

        public Quest(Player ladyOfTheLakeHolder, int teamSize, int requiredFails)
        {
            QuestSize = teamSize;
            RequiredFails = requiredFails;

            if (ladyOfTheLakeHolder != null)
            {
                LadyOfTheLake = new LadyOfTheLakeUse { Holder = ladyOfTheLakeHolder };
            }

            VoteTracks = new List<VoteTrack>();
        }
    }
}
