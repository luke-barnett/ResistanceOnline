using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    public class LadyOfTheLakeUse
    {
        public Player UsedBy { get; set; }
        public Player UsedOn { get; set; }
        public bool ResultWasEvil { get; set; }
        public int UsedOnRoundNumber { get; set; }
    }
}
