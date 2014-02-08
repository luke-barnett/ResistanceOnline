using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Site.Models
{
	public class TeamModel
	{
		public string Leader { get; set; }
		public List<string> TeamMembers { get; set; }
		public List<VoteModel> Vote { get; set; }
		public List<QuestModel> QuestCards { get; set; }

		public TeamModel(Core.Team quest, int totalPlayers)
		{
			Leader = quest.Leader.Name;

			TeamMembers = quest.TeamMembers.Select(p => p.Name).ToList();
			Vote = quest.Votes.OrderBy(q => Guid.NewGuid()).Select(v => new VoteModel(v, quest.Votes.Count != totalPlayers)).ToList();
			QuestCards = quest.Quests.OrderBy(q => Guid.NewGuid()).Select(q => new QuestModel(q.Success, quest.Quests.Count != TeamMembers.Count)).ToList();
		}
	}
}
