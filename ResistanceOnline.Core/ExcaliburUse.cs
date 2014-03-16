using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    public class ExcaliburUse
    {
        public Player Holder { get; set; }
        public QuestCard UsedOn { get; set; }
        public bool? OriginalMissionWasSuccess { get; set; }
    }
}
