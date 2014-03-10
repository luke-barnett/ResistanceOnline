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
        public Game Game { get; set; }
        public string Type { get; set; }
        public Player Owner { get; set; }
        public Player Target { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Text { get; set; }
    }
}
