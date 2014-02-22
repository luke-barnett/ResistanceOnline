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
            ProposingPlayers,
            Voting,
            Questing,
            Succeeded,
            Failed,
            FailedAllVotes
        }

        List<Player> _players;
        int _currentPlayer;

        public Round(List<Player> players, int currentPlayer, int teamSize, int requiredFails)
        {
            TotalPlayers = players.Count;
            TeamSize = teamSize;
            RequiredFails = requiredFails;

            Teams = new List<Team>();
            Teams.Add(new Team(players[currentPlayer]));

            _players = players;
            _currentPlayer = currentPlayer;
        }

        public int TotalPlayers { get; set; }
        public int TeamSize { get; set; }
        public int RequiredFails { get; set; }
        public int NextPlayer { get { return (_currentPlayer + 1) % TotalPlayers; } }

        public Team CurrentTeam { get { return Teams.Last(); } }

        public List<Team> Teams { get; set; }

        public void AddToTeam(Player player, Player proposedPlayer)
        {
            CurrentTeam.AddToTeam(player, proposedPlayer);
        }

        public void AssignExcalibur(Player player, Player proposedPlayer)
        {
            CurrentTeam.AssignExcalibur(player, proposedPlayer);
        }

        public void VoteForTeam(Player player, bool approve)
        {
            CurrentTeam.VoteForTeam(player, approve);

            //on the last vote, if it fails, create the next quest
            if (CurrentTeam.Votes.Count == TotalPlayers)
            {
                var rejects = CurrentTeam.Votes.Where(v => !v.Approve).Count();
                if (rejects >= Math.Ceiling(TotalPlayers / 2.0))
                {
                    _currentPlayer = NextPlayer;
                    Teams.Add(new Team(_players[_currentPlayer]));
                }
            }
        }

        public void SubmitQuest(Player player, bool success)
        {
            CurrentTeam.SubmitQuest(player, success);
        }

        public State DetermineState()
        {
            //no more than 5 quest votes per round
            if (Teams.Count > 5)
                return State.FailedAllVotes;
            
            //proposing
            if (CurrentTeam.TeamMembers.Count < TeamSize)
                return State.ProposingPlayers;

            //voting on proposing
            if (CurrentTeam.Votes.Count < TotalPlayers)
                return State.Voting;

            //questing
            if (CurrentTeam.Quests.Count < TeamSize)
                return State.Questing;

            //finished
            var fails = CurrentTeam.Quests.Where(c => !c.Success).Count();
            if (fails >= RequiredFails)
                return State.Failed;

            return State.Succeeded;           
        }

    }
}
