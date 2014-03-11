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
        public bool? Approve { get; set; }

        public string Image
        {
            get
            {
                if (!Approve.HasValue)
                    return "vote";
                if (Hidden)
                    return "vote";
                if (Approve.Value)
                    return "voteapprove";
                return "votereject";
            }
        }

        public VoteModel(Core.VoteToken vote, bool hidden)
        {
            Player = vote.Player.Name;
            Approve = vote.Approve;
            Hidden = hidden;            
        }
    }
}
