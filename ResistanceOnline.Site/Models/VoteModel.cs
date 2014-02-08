using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Site.Models
{
    public class VoteModel
    {
        public string Player { get; set; }
        public bool Hidden { get; set; }
        public bool Approve { get; set; }

        public string Image
        {
            get
            {
                if (Hidden)
                    return "vote";
                if (Approve)
                    return "voteapprove";
                return "votereject";
            }
        }

        public VoteModel(Core.Vote vote, bool hidden)
        {
            if (!hidden)
                Player = vote.Player.Name;
            Approve = vote.Approve;
            Hidden = hidden;            
        }
    }
}
