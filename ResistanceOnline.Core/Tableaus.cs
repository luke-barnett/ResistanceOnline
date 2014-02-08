using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Tableaus
    {
        public int TeamSize { get; set; }
        public int RequiredFails { get; set; }

        public Tableaus(int teamSize, int requiredFails=1)
        {
            TeamSize = teamSize;
            RequiredFails = requiredFails;
        }
    }
}
