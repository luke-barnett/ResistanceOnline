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

        public ExcaliburUseModel(ExcaliburUse use, Player player)
        {
            UsedBy = use.Holder.Name;
            UsedOn = use.UsedOn != null ? use.UsedOn.Player.Name : "No One";
            if (player == use.Holder)
            {
                Result = use.OriginalMissionWasSuccess.HasValue ? (use.OriginalMissionWasSuccess.Value ? "evil" : "good") : "";
            }
            else
            {
                Result = "allegiance";
            }
        }
    }
}
