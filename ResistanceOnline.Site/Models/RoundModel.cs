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
        public List<QuestModel> Quests { get; set; }

        public RoundModel(Core.Round round)
        {
            TeamSize = round.TeamSize;
            FailsRequired = round.RequiredFails;

            Quests = new List<QuestModel>();
            foreach (var quest in round.Quests)
            {
                Quests.Add(new QuestModel(quest, round.TotalPlayers));
            }
        }
    }
}