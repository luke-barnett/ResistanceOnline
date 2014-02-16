using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Humanizer;

namespace ResistanceOnline.Site.Models
{
    public class RoundModel
    {
        public int TeamSize { get; set; }
        public int FailsRequired { get; set; }
        public List<TeamModel> Teams { get; set; }
        private int _roundNumber;
        public string Title { get { return String.Format("Round {0}", _roundNumber.ToWords()); } }
        public string Summary { get { return String.Format("This is a {0} player round and requires {1}", TeamSize.ToWords(), "fail".ToQuantity(FailsRequired, ShowQuantityAs.Words)); } }

        public RoundModel(Core.Round round, int roundNumber)
        {
            TeamSize = round.TeamSize;
            FailsRequired = round.RequiredFails;
            _roundNumber = roundNumber;

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