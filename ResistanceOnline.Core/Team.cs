using System;
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
        public enum State
        {
            ChoosingTeam,
            AssigningExcalibur,
            VotingForTeam,
            VoteFailed,
            Questing,
            UsingExcalibur,
            Finished,
        };

        public List<Rule> Rules { get; set; }
        public Player Leader { get; set; }
        public ExcaliburUse Excalibur { get; set; }
        public List<Player> TeamMembers { get; set; }
        public List<Vote> Votes { get; set; }
        public List<Quest> Quests { get; set; }
        public List<PlayerMessage> Messages { get; set; }
        public int TeamSize { get; set; }
        public int RequiredFails { get; set; }
        public State TeamState { get; set; }
        public int GameSize { get; set; }

        public event EventHandler Finished;
        private void OnFinished()
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        public Team(Player leader, List<Player> players, int size, int requiredFails, List<Rule> rules)
        {
            Leader = leader;
            GameSize = players.Count;
            TeamSize = size;
            RequiredFails = requiredFails;
            TeamMembers = new List<Player>();
            Votes = players.Select(p=>new Vote { Player = p }).ToList();
            Quests = new List<Quest>();
            Messages = new List<PlayerMessage>();
        }

        public List<Action.Type> AvailableActions(Player player)
        {
            var actions = new List<Action.Type>() { };
            switch (TeamState)
            {
                case State.ChoosingTeam:
                    if (player == Leader)
                    {
                        actions.Add(Action.Type.AddToTeam);
                    }
                    break;
                case State.AssigningExcalibur:
                    if (player == Leader)
                    {
                        actions.Add(Action.Type.AddToTeam);
                    }
                    break;
                case State.VotingForTeam:
                    if (Votes.Where(v=>!v.Approve.HasValue).Select(v => v.Player).Contains(player))
                    {
                        actions.Add(Action.Type.VoteForTeam);
                    }
                    break;
                case State.Questing:
                    if (Quests.Where(q => !q.Success.HasValue).Select(q => q.Player).Contains(player))
                    {
                        actions.Add(Action.Type.SubmitQuestCard);
                    }
                    break;
                case State.UsingExcalibur:
                    if (Excalibur.Holder == player)
                    {
                        actions.Add(Action.Type.UseExcalibur);
                    }
                    break;
            }
            return actions;
        }

        public void AddToTeam(Player player, Player proposedPlayer)
        {
            if (TeamMembers.Contains(proposedPlayer))
                throw new Exception("Hax. Player is already on the team");

            if (player != Leader)
                throw new Exception("Hax. Player is not the leader of this team");

            TeamMembers.Add(proposedPlayer);

            if (TeamMembers.Count == TeamSize)
            {
                if (Rules != null && Rules.Contains(Rule.IncludeExcalibur))
                {
                    TeamState = State.AssigningExcalibur;
                }
                else
                {
                    TeamState = State.VotingForTeam;
                }
            }
        }

        public void AssignExcalibur(Player player, Player proposedPlayer)
        {
            if (player != Leader)
                throw new Exception("Hax. Player is not the leader of this team");

            if (!TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is not on team..");

            if (proposedPlayer == Leader)
                throw new Exception("Leader cannot assigne excalibur to themself");

            Excalibur.Holder = proposedPlayer;

            TeamState = State.VotingForTeam;
        }

        public void UseExcalibur(Player player, Player proposedPlayer)
        {
            if (player != Excalibur.Holder)
                throw new Exception("Hax. Player does not have excalibur");

            if (proposedPlayer != null && !TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is not on team..");

            Excalibur.UsedOn = Quests.First(p => p.Player == proposedPlayer);
            Excalibur.OriginalMissionWasSuccess = Excalibur.UsedOn.Success;
            Excalibur.UsedOn.Success = !Excalibur.UsedOn.Success;

            TeamState = State.Finished;

            OnFinished();
        }

        public bool? IsSuccess
        {
            get
            {
                if (Quests.Count(c => c.Success.HasValue) < TeamSize)
                    return null;
                return Quests.Count(c => !c.Success.Value) < RequiredFails;
            }
        }

        public void VoteForTeam(Player player, bool approve)
        {
            var vote = Votes.FirstOrDefault(v => v.Player == player);
            if (vote == null)
                throw new Exception("Player should not be voting");
            if (vote.Approve.HasValue)
                throw new Exception("Player has already voted");

            vote.Approve = approve;

            //on the last vote, if it fails, create the next quest
            if (Votes.Any(v => !v.Approve.HasValue))
                return;

            var rejects = Votes.Count(v => !v.Approve.Value);
            if (rejects >= Math.Ceiling(GameSize / 2.0))
            {
                TeamState = State.VoteFailed;
            }
            else
            {
                TeamState = State.Questing;
            }
        }

        public void SubmitQuest(Player player, bool success)
        {
            if (Quests.Select(v => v.Player).ToList().Contains(player))
                throw new Exception("Player has already submitted their quest card..");

            Quests.Add(new Quest { Player = player, Success = success });

            if (Quests.Any(q => !q.Success.HasValue))
                return;

            TeamState = State.Finished;
        }
    }
}

