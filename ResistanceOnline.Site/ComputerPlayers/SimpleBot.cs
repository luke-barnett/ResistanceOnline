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
        public SimpleBot(Game game, Guid playerGuid) : base(game,playerGuid) {  }

        private int CountEvil(List<Player> list, Game game)
        {
            var evilCount = 0;
            foreach (var player in list)
            {
                if (IKnowTheyAreEvil(player,game))
                {
                    evilCount++;
                }
            }
            return evilCount;
        }

        private bool IKnowTheyAreEvil(Player player, Game game)
        {
            var knowledge = game.PlayerKnowledge(_player, player);
            if (knowledge == Knowledge.Evil || (knowledge == Knowledge.EvilLancelot && !game.LancelotAllegianceSwitched) || (knowledge == Knowledge.Lancelot && game.LancelotAllegianceSwitched))
            {
                return true;
            }
            return false;
        }

        protected override Core.Player LadyOfTheLakeTarget()
        {
            if (_game.LadyOfTheLakeUses.Count == 0)
            {
                return _game.Players.First(p => p != _player);
            }

            return _game.LadyOfTheLakeUses.Select(u => u.UsedBy).Where(p => p != _player).First();
        }

        protected override Core.Player GuessMerlin()
        {
            return _game.Players.FirstOrDefault(p => p != _player && IKnowTheyAreEvil(p, _game) == false);
        }

        protected override Core.Player ChooseTeamPlayer()
        {
            //put myself on
            if (!_game.CurrentRound.CurrentTeam.TeamMembers.Any(p => p == _player))
            {
                return _player;
            }

            var playersNotOnTeam = _game.Players.Where(p => p != _player).Except(_game.CurrentRound.CurrentTeam.TeamMembers);

            //if I'm evil, put anyone else on
            if (_IAmEvil)
            {
                return playersNotOnTeam.First();
            }

            //if I'm good, try not to put evil on the mission
            var player = playersNotOnTeam.FirstOrDefault(p=>IKnowTheyAreEvil(p, _game) == false);
            //failing that we need to put someone evil on the mission :(
            if (player == null)
            {
                player = playersNotOnTeam.First();
            }
            return player;
        }

        protected override bool Quest()
        {
            return _IAmEvil == false;
        }

        protected override bool TeamVote()
        {
            var evilCount = CountEvil(_game.CurrentRound.CurrentTeam.TeamMembers, _game);
            if (_IAmEvil)
            {
                if (evilCount > 0)
                    return true;
                return false;
            }
            else
            {
                if (evilCount > 0)
                    return false;
                return true;
            }

        }
    }
        
}
