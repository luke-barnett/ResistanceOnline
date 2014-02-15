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
            Say("I'm just going use the lady of the lake on someone other than me.");

            if (_game.LadyOfTheLakeUses.Count == 0)
            {
                return _game.Players.Random(p => p != _player);
            }

            return _game.LadyOfTheLakeUses.Select(u => u.UsedBy).Where(p => p != _player).First();
        }

        protected override Core.Player GuessMerlin()
        {
            Say("I'm just going to guess anyone I know isn't evil..");
            return _game.Players.Shuffle().FirstOrDefault(p => p != _player && IKnowTheyAreEvil(p, _game) == false);
        }

        protected override Core.Player ChooseTeamPlayer()
        {
            //put myself on
            if (!_game.CurrentRound.CurrentTeam.TeamMembers.Any(p => p == _player))
            {
                Say("Obviously I need to be on this mission");
                return _player;
            }

            var playersNotOnTeam = _game.Players.Where(p => p != _player).Except(_game.CurrentRound.CurrentTeam.TeamMembers);
            Player player = null;

            //if I'm evil, put anyone else on
            if (_IAmEvil)
            {
                player = playersNotOnTeam.Random();                
                return player;
            }

            //if I'm good, try not to put evil on the mission
            player = playersNotOnTeam.FirstOrDefault(p=>IKnowTheyAreEvil(p, _game) == false);
            //failing that we need to put someone evil on the mission :(
            if (player == null)
            {
                player = playersNotOnTeam.Random();
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
            var evilCount = CountEvil(_game.CurrentRound.CurrentTeam.TeamMembers, _game);
            if (_IAmEvil)
            {
                if (evilCount >= _game.CurrentRound.RequiredFails)
                {
                    Say("I like this team");
                    return true;
                }
                Say("This could be ok, but I'm not sure");
                return false;
            }
            else
            {
                if (evilCount >= _game.CurrentRound.RequiredFails)
                {
                    Say("I really don't like this");
                    return false;
                }
                Say("I like this team");
                return true;
            }

        }
    }
        
}
