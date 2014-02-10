using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class RoundTable
    {
        public int TeamSize { get; set; }
        public int RequiredFails { get; set; }

        public RoundTable(int teamSize, int requiredFails=1)
        {
            TeamSize = teamSize;
            RequiredFails = requiredFails;
        }
    }
}
