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
        public LadyOfTheLakeUseModel LadyOfTheLake { get; set; }
        public string LoyaltyCard { get; set; }
        public string Outcome { get; set; }

        public RoundModel(Core.Round round, int roundNumber, Core.Game game, Core.Player player)
        {
            TeamSize = round.TeamSize;
            FailsRequired = round.RequiredFails;
            _roundNumber = roundNumber;

            if (round.LadyOfTheLake!=null) 
            {
                LadyOfTheLake = new LadyOfTheLakeUseModel(round.LadyOfTheLake, round.LadyOfTheLake.Target);
            }
            
            Teams = new List<TeamModel>();
            foreach (var team in round.Teams)
            {
                Teams.Add(new TeamModel(team, round.Players.Count, round.Teams.IndexOf(team)+1));
            }

            if (round.IsSuccess.HasValue)
            {
                Outcome = round.IsSuccess.Value ? "good-wins" : "evil-wins";
            }

            var loyaltyCard = game.Setup.GetLoyaltyCard(roundNumber);
            if (loyaltyCard.HasValue && round != game.CurrentRound)
            {
                LoyaltyCard = string.Format("Lancelot loyalty card: {0}", loyaltyCard.Value.Humanize());
            }

        }

    }
}