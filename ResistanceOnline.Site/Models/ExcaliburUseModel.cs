using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Site.Models
{
    public class ExcaliburUseModel
    {
        public string UsedBy { get; set; }
        public string UsedOn { get; set; }
        public string Result { get; set; }
        public int UsedOnRoundNumber { get; set; }

        public ExcaliburUseModel(ExcaliburUse use, Player player)
        {
            UsedBy = use.UsedBy.Name;
            UsedOn = use.UsedOn.Name;
            UsedOnRoundNumber = use.UsedOnRoundNumber;
            if (player == use.UsedBy)
            {
                Result = use.OriginalMissionWasSuccess ? "evil" : "good";
            }
            else
            {
                Result = "allegiance";
            }
        }
    }
}
