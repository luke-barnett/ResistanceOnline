using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// A quest that get's voted on then may go ahead
    /// </summary>
    public class Quest
    {
        public Quest(Player leader)
        {
            Leader = leader;
            ProposedPlayers = new List<Player>();
            Votes = new List<QuestVote>();
            QuestCards = new List<QuestCard>();
        }

        public void ProposePlayer(Player proposedPlayer) {
            if (ProposedPlayers.Contains(proposedPlayer))
                throw new Exception("Player is already on quest..");

            ProposedPlayers.Add(proposedPlayer);
        }

        public void VoteForQuest(Player player, bool approve)
        {
            if (Votes.Select(v=>v.Player).ToList().Contains(player))
                throw new Exception("Player has already voted..");

            Votes.Add(new QuestVote { Player = player, Approve = approve });
        }

        public void SubmitQuest(Player player, bool success)
        {
            if (QuestCards.Select(v => v.Player).ToList().Contains(player))
                throw new Exception("Player has already submitted their quest card..");

            QuestCards.Add(new QuestCard { Player = player, Success = success });
        }

        public Player Leader { get; set; }
        public List<Player> ProposedPlayers { get; set; }
        public List<QuestVote> Votes { get; set; }
        public List<QuestCard> QuestCards { get; set; }
    }
}
