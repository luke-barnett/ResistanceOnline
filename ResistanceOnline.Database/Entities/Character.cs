using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Database.Entities
{
    public class Character
    {
        public int CharacterId { get; set; }
        public string Name { get; set; }
        public Game Game { get; set; }
    }
}
