using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Database.Entities
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
		public string Character { get; set; }
        public string Type { get; set; }
        public Guid Guid { get; set; }
        public Game Game { get; set; }        
    }
}
