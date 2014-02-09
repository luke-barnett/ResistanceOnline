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

            var state = round.DetermineState();
            if (state == Core.Round.State.Failed || state == Core.Round.State.FailedAllVotes)
                Outcome = "evil-wins";
            if (state == Core.Round.State.Succeeded)
                Outcome = "good-wins";
        }

        public string Outcome { get; set; }
    }
}