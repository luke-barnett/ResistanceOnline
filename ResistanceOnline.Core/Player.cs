using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    [DebuggerDisplay("{Name} {Character}")]
    public class Player
    {
        public enum Type
        {
            Human,
            TrustBot
        }

        public string Name { get; set; }
        public Character Character { get; set; }
        public Guid Guid { get; set; }
        public Type PlayerType { get; set; }

    }
}
