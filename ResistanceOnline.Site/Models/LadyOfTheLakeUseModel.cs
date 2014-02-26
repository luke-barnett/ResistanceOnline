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

        public LadyOfTheLakeUseModel(LadyOfTheLakeUse use, Player player)
        {
            UsedBy = use.Holder.Name;
            UsedOn = use.Target.Name;
            if (player == use.Holder)
            {
                Result = use.IsEvil ? "evil" : "good";
            }
            else
            {
                Result = "allegiance";
            }
        }
    }
}
