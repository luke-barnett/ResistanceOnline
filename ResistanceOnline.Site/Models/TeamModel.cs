using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;

namespace ResistanceOnline.Site.Models
{
	public class TeamModel
	{
		public string Leader { get; set; }
        public ExcaliburUseModel Excalibur { get; set; }
		public List<string> TeamMembers { get; set; }
		public List<VoteModel> Vote { get; set; }
		public List<QuestCardModel> QuestCards { get; set; }
        public List<MessageModel> Messages { get; set; }
        public string TeamSummary { get; set; }
        public TeamModel(Core.VoteTrack team, int totalPlayers, int teamNumber)
		{
            if (team.Leader != null)
            {
                Leader = team.Leader.Name;
                TeamSummary = string.Format("Team {0}, as proposed by {1}", teamNumber.ToWords(), Leader);
            }
            if (team.Excalibur != null)
            {
                Excalibur = new ExcaliburUseModel(team.Excalibur, team.Excalibur.Holder);
            }
			TeamMembers = team.Players.Select(p => p.Name).ToList();
			Vote = team.Votes.OrderBy(v=>v.Player.Name).Select(v => new VoteModel(v, team.Votes.Count != totalPlayers)).ToList();
			QuestCards = team.QuestCards.OrderBy(q=> q.Success).Select(q => new QuestCardModel(q.Success, team.QuestCards.Count != TeamMembers.Count)).ToList();
            Messages = team.Messages.Select(m => new MessageModel(m.Player.Name, m.Message)).ToList();
		}
	}
}
