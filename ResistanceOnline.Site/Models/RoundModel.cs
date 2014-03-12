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
        public string Title { get; set; }
        public string Summary { get { return String.Format("This is a {0} player quest and requires {1}", TeamSize.ToWords(), "fail".ToQuantity(FailsRequired, ShowQuantityAs.Words)); } }

        public string LadyOfTheLakeUsedBy { get; set; }
        public string LadyOfTheLakeUsedOn { get; set; }
        public string LadyOfTheLakeResult { get; set; }


        public string LoyaltyCard { get; set; }
        public string Outcome { get; set; }

        public RoundModel(Core.Quest round, int roundNumber, Core.Game game, Core.Player player)
        {
            TeamSize = round.TeamSize;
            FailsRequired = round.RequiredFails;
            _roundNumber = roundNumber;

            Title = String.Format("Quest {0}", _roundNumber.ToWords());
            if (round.IsSuccess.HasValue && round.IsSuccess.Value)
            {
                Title += " succeeded";
            }
            if (round.IsSuccess.HasValue && !round.IsSuccess.Value)
            {
                Title += " failed";
            }

            if (round.LadyOfTheLake!=null && round.LadyOfTheLake.Target!=null) 
            {
                LadyOfTheLakeUsedBy = round.LadyOfTheLake.Holder.Name;
                LadyOfTheLakeUsedOn = round.LadyOfTheLake.Target.Name;

                if (player == round.LadyOfTheLake.Holder)
                {
                    LadyOfTheLakeResult = round.LadyOfTheLake.IsEvil ? "evil" : "good";
                }
                else
                {
                    LadyOfTheLakeResult = "allegiance";
                }
            }
            
            Teams = new List<TeamModel>();
            foreach (var team in round.VoteTracks)
            {
                Teams.Add(new TeamModel(team, round.Players.Count, round.VoteTracks.IndexOf(team)+1));
            }

            var loyaltyCard = game.GetLoyaltyCard(roundNumber);
            if (loyaltyCard.HasValue && round != game.CurrentQuest)
            {
                LoyaltyCard = string.Format("Lancelot loyalty card: {0}", loyaltyCard.Value.Humanize());
            }

        }

    }
}