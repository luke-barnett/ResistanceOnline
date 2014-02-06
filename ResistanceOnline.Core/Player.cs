using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Player
    {
        public string Name { get; set; }
        public Character Character { get; set; }

        public Guid Guid { get; set; }
    }
}
