using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// A set of up to 5 quests that get proposed and voted on
    /// </summary>
    public class Round
    {
        public enum State
        {
            Unstarted,
            InProgress,
            FailedAllVotes,
            Finished,
            LadyOfTheLake
        }

        public int TeamSize { get; set; }
        public int RequiredFails { get; set; }
        public List<Team> Teams { get; set; }
        public Team CurrentTeam { get { return Teams.Last(); } }
        public LadyOfTheLakeUse LadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public State RoundState { get; set; }
        public List<Player> Players { get; set; }
        public List<Rule> Rules { get; set; }

        public event EventHandler Finished;
        private void OnFinished(bool isSuccess)
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        public Round(List<Player> players, Player leader, Player ladyOfTheLakeHolder, int teamSize, int requiredFails, List<Rule> rules, bool lancelotAllegianceSwitched)
        {
            RoundState = State.Unstarted;
            TeamSize = teamSize;
            RequiredFails = requiredFails;

            if (ladyOfTheLakeHolder != null)
            {
                LadyOfTheLake = new LadyOfTheLakeUse { Holder = ladyOfTheLakeHolder };
            }

            Players = players;            
            Rules = rules ?? new List<Rule>();
            Teams = new List<Team>();
            NextTeam(leader);
            RoundState = State.InProgress;
        }

        public List<Action.Type> AvailableActions(Player player)
        {
            //lady of the lake
            switch (RoundState)
            {
                case State.InProgress:
                    return CurrentTeam.AvailableActions(player);
                case State.LadyOfTheLake:
                    if (Rules.Contains(Rule.IncludeLadyOfTheLake) && LadyOfTheLake != null && LadyOfTheLake.Holder == player && LadyOfTheLake.Target == null)
                    {
                        return new List<Action.Type>() { Action.Type.UseTheLadyOfTheLake };
                    }
                    break;
            }

            return new List<Action.Type>();            
        }

        public void AddToTeam(Player player, Player proposedPlayer)
        {
            CurrentTeam.AddToTeam(player, proposedPlayer);
        }

        public void AssignExcalibur(Player player, Player proposedPlayer)
        {
            CurrentTeam.AssignExcalibur(player, proposedPlayer);
        }

        public void UseExcalibur(Player player, Player proposedPlayer)
        {
            CurrentTeam.UseExcalibur(player, proposedPlayer);            
        }

        public void VoteForTeam(Player player, bool approve)
        {
            CurrentTeam.VoteForTeam(player, approve);
        }

        public void SubmitQuest(Player player, bool success)
        {
            CurrentTeam.SubmitQuest(player, success);
        }

        private void NextTeam(Player player)
        {
            var team = new Team(player, Players, TeamSize, RequiredFails, Rules);
            team.Finished += Team_Finished;
            Teams.Add(team);
        }

        private void Team_Finished(object sender, EventArgs e)
        {
            if (CurrentTeam.TeamState == Team.State.VoteFailed)
            {
                if (Teams.Count == 5)
                {
                    RoundState = State.FailedAllVotes;
                    OnFinished(false);
                }
                else
                {
                    NextTeam(Players.Next(CurrentTeam.Leader));
                }
            }

            if (CurrentTeam.TeamState == Team.State.Finished)
            {
                RoundState = State.Finished;
                OnFinished(IsSuccess.Value);
            }
        }

        public bool? IsSuccess
        {
            get
            {
                return CurrentTeam.IsSuccess;
            }
        }

        internal void UseLadyOfTheLake(Player player, Player target)
        {
            if (RoundState != State.LadyOfTheLake)
                throw new Exception("wrong state for this action");

            if (LadyOfTheLake!=null && LadyOfTheLake.Holder != player)
                throw new Exception("Hax. Player does not have lady of the lake.");

            LadyOfTheLake.Target = target;
            LadyOfTheLake.IsEvil = IsCharacterEvil(target.Character, LancelotAllegianceSwitched);

            RoundState = State.Finished;
            OnFinished(IsSuccess.Value);
        }

        internal bool IsCharacterEvil(Character character, bool lancelotAllegianceSwitched)
        {
            switch (character)
            {
                case Core.Character.Assassin:
                case Core.Character.MinionOfMordred:
                case Core.Character.Mordred:
                case Core.Character.Morgana:
                case Core.Character.Oberon:
                    return true;
                case Core.Character.Lancelot:
                    if (lancelotAllegianceSwitched)
                    {
                        return true;
                    }
                    break;
                case Core.Character.EvilLancelot:
                    if (!lancelotAllegianceSwitched)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
