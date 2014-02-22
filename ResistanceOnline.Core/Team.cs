﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// A quest that get's voted on then may go ahead
    /// </summary>
    public class Team
    {
        public Team(Player leader)
        {
            Leader = leader;
            TeamMembers = new List<Player>();
            Votes = new List<Vote>();
            Quests = new List<Quest>();
            Messages = new List<PlayerMessage>();
        }

        public void AddToTeam(Player player, Player proposedPlayer)
        {
            if (TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is already on team..");

            if (player != Leader)
                throw new Exception("Hax. Player is not the leader of this team");

            TeamMembers.Add(proposedPlayer);
        }

        public void AssignExcalibur(Player player, Player proposedPlayer)
        {
            if (player != Leader)
                throw new Exception("Hax. Player is not the leader of this team");

            if (!TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is not on team..");

            if (proposedPlayer == Leader)
                throw new Exception("Leader cannot assigne excalibur to themself");

            HasExcalibur = proposedPlayer;
        }

        public bool? UseExcalibur(Player player, Player proposedPlayer)
        {
            if (player != HasExcalibur)
                throw new Exception("Hax. Player does not have excalibur");

            if (proposedPlayer != null && !TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is not on team..");

            ExcaliburUsed = true;
            if (proposedPlayer != null)
            {
                var quest = Quests.SingleOrDefault(p => p.Player == proposedPlayer);
                quest.Success = !quest.Success;
                return !quest.Success; //show original value
            }

            return null;
        }

        public void VoteForTeam(Player player, bool approve)
        {
            if (Votes.Select(v=>v.Player).ToList().Contains(player))
                throw new Exception("Player has already voted..");

            Votes.Add(new Vote { Player = player, Approve = approve });
        }

        public void SubmitQuest(Player player, bool success)
        {
            if (Quests.Select(v => v.Player).ToList().Contains(player))
                throw new Exception("Player has already submitted their quest card..");

            Quests.Add(new Quest { Player = player, Success = success });
        }

        public Player Leader { get; set; }
        public Player HasExcalibur { get; set; }
        public bool ExcaliburUsed { get; set; }

        public List<Player> TeamMembers { get; set; }
        public List<Vote> Votes { get; set; }
        public List<Quest> Quests { get; set; }

        public List<PlayerMessage> Messages { get; set; }

    }
}

