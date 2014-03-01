using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Round
    {
        public int TeamSize { get; set; }
        public int RequiredFails { get; set; }
        public List<Team> Teams { get; set; }
        public Team CurrentTeam { get { return Teams.Last(); } }
        public LadyOfTheLakeUse LadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public List<Player> Players { get; set; }
        public List<Rule> Rules { get; set; }

        public bool? IsSuccess
        {
            get
            {
                return CurrentTeam.IsSuccess;
            }
        }   

        public Round(List<Player> players, Player leader, Player ladyOfTheLakeHolder, int teamSize, int requiredFails, bool lancelotAllegianceSwitched)
        {
            TeamSize = teamSize;
            RequiredFails = requiredFails;

            if (ladyOfTheLakeHolder != null)
            {
                LadyOfTheLake = new LadyOfTheLakeUse { Holder = ladyOfTheLakeHolder };
            }

            Players = players;            
            Teams = new List<Team>();
        }
    }
}
