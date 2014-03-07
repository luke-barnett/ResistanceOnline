using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Site.ComputerPlayers
{
    public class SimpleBot : ComputerPlayer
    {
        public SimpleBot(GamePlay game, Guid playerGuid) : base(game,playerGuid) {  }

        private int CountEvil(List<Player> list, GamePlay gameplay)
        {
            var evilCount = 0;
            foreach (var player in list)
            {
                if (IKnowTheyAreEvil(player,gameplay))
                {
                    evilCount++;
                }
            }
            return evilCount;
        }

        private bool IKnowTheyAreEvil(Player player, GamePlay gameplay)
        {
            var knowledge = gameplay.PlayerKnowledge(_player, player);
            if (knowledge == Knowledge.Evil || (knowledge == Knowledge.EvilLancelot && !gameplay.LancelotAllegianceSwitched) || (knowledge == Knowledge.Lancelot && gameplay.LancelotAllegianceSwitched))
            {
                return true;
            }
            return false;
        }

        protected override Core.Player LadyOfTheLakeTarget()
        {
            var ladyOfTheLakeHistory = _gameplay.Rounds.Where(r => r.LadyOfTheLake != null).Select(r => r.LadyOfTheLake.Holder);
            return _gameplay.Game.Players.Where(p => p != _player).Except(ladyOfTheLakeHistory).Random();
        }

        protected override Core.Player GuessMerlin()
        {
            Say("I'm just going to guess anyone I know isn't evil..");
            return _gameplay.Game.Players.RandomOrDefault(p => p != _player && IKnowTheyAreEvil(p, _gameplay) == false);
        }

        protected override Core.Player ChooseTeamPlayer()
        {
            //put myself on
            if (!_gameplay.CurrentRound.CurrentTeam.TeamMembers.Any(p => p == _player))
            {
                return _player;
            }

            var playersNotOnTeam = _gameplay.Game.Players.Where(p => p != _player).Except(_gameplay.CurrentRound.CurrentTeam.TeamMembers);
            Player player = null;

            //if I'm evil, put anyone else on
            if (_IAmEvil)
            {
                player = playersNotOnTeam.Random();
                SayTheyAreGood(player.Name);
                return player;
            }

            //if I'm good, try not to put evil on the mission
            player = playersNotOnTeam.RandomOrDefault(p => IKnowTheyAreEvil(p, _gameplay) == false);
            //failing that we need to put someone evil on the mission :(
            if (player == null)
            {
                player = playersNotOnTeam.Random();
                SayTheyAreGood(player.Name);
            }
            else
            {
                SayTheyAreGood(player.Name);
            }
            return player;
        }

        protected override bool Quest()
        {
            return _IAmEvil == false;
        }

        protected override bool TeamVote()
        {
            var evilCount = CountEvil(_gameplay.CurrentRound.CurrentTeam.TeamMembers, _gameplay);
            if (_IAmEvil)
            {
                if (evilCount >= _gameplay.CurrentRound.RequiredFails)
                {
                    Say("I like this team");
                    return true;
                }
                Say("This could be ok, but I'm not sure");
                return false;
            }
            else
            {
                if (evilCount >= _gameplay.CurrentRound.RequiredFails)
                {
                    Say("I really don't like this");
                    return false;
                }
                Say("I like this team");
                return true;
            }

        }

        protected override Player UseExcalibur()
        {
            return null;
        }
    }
        
}
