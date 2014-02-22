using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    public class ExcaliburUse
    {
        public Player UsedBy { get; set; }
        public Player UsedOn { get; set; }
        public bool? OriginalMissionWasSuccess { get; set; }
        public int UsedOnRoundNumber { get; set; }
    }
}
