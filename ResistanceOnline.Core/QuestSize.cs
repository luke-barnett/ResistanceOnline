using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class QuestSize
    {
        public int TeamSize { get; set; }
        public int RequiredFails { get; set; }

        public QuestSize(int teamSize, int requiredFails=1)
        {
            TeamSize = teamSize;
            RequiredFails = requiredFails;
        }

		public QuestSize(Database.Entities.Round round)
		{
			TeamSize = round.Size;
			RequiredFails = round.Fails;
		}
    }
}
