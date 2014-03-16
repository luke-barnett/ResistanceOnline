using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Database.Entities
{
    public class Action
    {
        public int ActionId { get; set; }
        public int GameId { get; set; }
        public string Type { get; set; }
        public Guid Owner { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Text { get; set; }
    }
}
