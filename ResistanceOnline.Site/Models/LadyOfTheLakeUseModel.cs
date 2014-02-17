using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Site.Models
{
    public class LadyOfTheLakeUseModel
    {
        public string UsedBy { get; set; }
        public string UsedOn { get; set; }
        public string Result { get; set; }
        public int UsedOnRoundNumber { get; set; }

        public LadyOfTheLakeUseModel(LadyOfTheLakeUse use, Player player)
        {
            UsedBy = use.UsedBy.Name;
            UsedOn = use.UsedOn.Name;
            UsedOnRoundNumber = use.UsedOnRoundNumber;
            if (player == use.UsedBy)
            {
                Result = use.ResultWasEvil ? "evil" : "good";
            }
            else
            {
                Result = "allegiance";
            }
        }
    }
}
