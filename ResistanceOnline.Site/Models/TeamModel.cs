﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Site.Models
{
	public class TeamModel
	{
		public string Leader { get; set; }
        public string HasExcalibur { get; set; }
		public List<string> TeamMembers { get; set; }
		public List<VoteModel> Vote { get; set; }
		public List<QuestModel> QuestCards { get; set; }
        public List<MessageModel> Messages { get; set; }

        public TeamModel(Core.Team team, int totalPlayers)
		{
			Leader = team.Leader.Name;
            HasExcalibur = team.HasExcalibur.Name;
			TeamMembers = team.TeamMembers.Select(p => p.Name).ToList();
			Vote = team.Votes.OrderBy(v=>v.Player.Name).Select(v => new VoteModel(v, team.Votes.Count != totalPlayers)).ToList();
			QuestCards = team.Quests.OrderBy(q=> q.Success).Select(q => new QuestModel(q.Success, team.Quests.Count != TeamMembers.Count)).ToList();
            Messages = team.Messages.Select(m => new MessageModel(m.Player.Name, m.Message)).ToList();
		}
	}
}
