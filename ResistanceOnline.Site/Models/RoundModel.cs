using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Site.Models
{
    public class RoundModel
    {
        public int TeamSize { get; set; }
        public int FailsRequired { get; set; }
        public List<TeamModel> Teams { get; set; }

        public RoundModel(Core.Round round)
        {
            TeamSize = round.TeamSize;
            FailsRequired = round.RequiredFails;

            Teams = new List<TeamModel>();
            foreach (var quest in round.Teams)
            {
                Teams.Add(new TeamModel(quest, round.TotalPlayers));
            }
        }
    }
}