using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Game
    {
        public int GameId { get; set; }
        public List<Player> Players { get; set; }
        public List<Quest> Quests { get; set; }
        public int QuestIndicator { get; set; }
    }
}
