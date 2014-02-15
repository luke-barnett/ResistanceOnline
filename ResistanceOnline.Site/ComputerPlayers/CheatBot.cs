using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using ResistanceOnline.Site;

namespace ResistanceOnline.Site.ComputerPlayers
{
    public class CheatBot : ComputerPlayer
    {
        public CheatBot(Game game, Guid playerGuid) : base(game, playerGuid) { }
        
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
            Say("Hahaha, I saw who merlin was at the beginning of the game!");
            return _game.Players.FirstOrDefault(p=> p.Character == Character.Merlin);
        }

        protected override Core.Player ChooseTeamPlayer()
        {
            //put myself on
            if (!_game.CurrentRound.CurrentTeam.TeamMembers.Any(p => p == _player))
            {
                return _player;
            }

            var playersNotOnTeam = _game.Players.Where(p => p != _player).Except(_game.CurrentRound.CurrentTeam.TeamMembers);
            Player player = null;

            //if I'm evil, put anyone else on
            if (_IAmEvil)
            {
                player = playersNotOnTeam.Random();
                SayTheyAreGood(player.Name);
                return player;
            }

            //if I'm good, only put good on
            player = playersNotOnTeam.FirstOrDefault(p=> !_game.IsCharacterEvil(p.Character));
            //failing that we need to put someone evil on the mission :(
            if (player == null)
            {
                player = playersNotOnTeam.First();
                SayTheyAreEvil(player.Name);
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
            var evilCount = _game.CurrentRound.CurrentTeam.TeamMembers.Count(p => _game.IsCharacterEvil(p.Character));

            if (_IAmEvil)
            {
                if (evilCount >= _game.CurrentRound.RequiredFails)
                {
                    SayTeamIsOk();
                    return true;
                }
                SayTeamNotOk();
                return false;
            }
            else
            {
                if (evilCount >= _game.CurrentRound.RequiredFails)
                {
                    SayTeamNotOk();
                    return false;
                }
                SayTeamIsOk();
                return true;
            }

        }
    }
        
}
