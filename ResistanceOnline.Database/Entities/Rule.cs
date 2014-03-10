using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Database.Entities
{
    public class Rule
    {
        public int RuleId { get; set; }
        public string Name { get; set; }
        public Game Game { get; set; }
    }
}
