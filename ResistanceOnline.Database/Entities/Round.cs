using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Database.Entities
{
    public class Round
    {
        public int RoundId { get; set; }
        public int Size { get; set; }
        public int Fails { get; set; }
        public Game Game { get; set; }
    }
}
