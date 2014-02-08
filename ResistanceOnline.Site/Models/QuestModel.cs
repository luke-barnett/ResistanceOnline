using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Site.Models
{
    public class QuestModel
    {
        public string Leader { get; set; }
        public List<string> PlayersOnQuest { get; set; }
        public List<VoteModel> Vote { get; set; }
        public List<QuestCardModel> QuestCards { get; set; }

        public QuestModel(Core.Quest quest, int totalPlayers)
        {
            Leader = quest.Leader.Name;

            PlayersOnQuest = quest.ProposedPlayers.Select(p => p.Name).ToList();
            Vote = quest.Votes.Select(v => new VoteModel(v, quest.Votes.Count != totalPlayers)).ToList();
            QuestCards = quest.QuestCards.Select(q => new QuestCardModel(q.Success, quest.QuestCards.Count != PlayersOnQuest.Count)).ToList();
        }
    }
}
