using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Database.Entities
{
    public class LoyaltyCard
    {
        public int LoyaltyCardId { get; set; }
        public string Card { get; set; }
        public Game Game { get; set; }
    }
}
