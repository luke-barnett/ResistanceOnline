using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core.ComputerPlayers
{
    public class SimpleBot : ComputerPlayer
    {
        public SimpleBot(Guid playerGuid) : base(playerGuid) {  }

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
            if (knowledge == Knowledge.Evil || (knowledge == Knowledge.EvilLancelot && !game.CurrentLancelotAllegianceSwitched) || (knowledge == Knowledge.Lancelot && game.CurrentLancelotAllegianceSwitched))
            {
                return true;
            }
            return false;
        }

        protected override Core.Player LadyOfTheLakeTarget()
        {
            var ladyOfTheLakeHistory = _game.Quests.Where(r => r.LadyOfTheLake != null).Select(r => r.LadyOfTheLake.Holder);
            return Random(_game.Players.Where(p => p != _player).Except(ladyOfTheLakeHistory));
        }

        protected override Core.Player GuessMerlin()
        {
            Say("I'm just going to guess anyone I know isn't evil..");
            return Random(_game.Players.Where(p => p != _player && IKnowTheyAreEvil(p, _game) == false));
        }

        protected override Core.Player ChooseTeamPlayer()
        {
            //put myself on
            if (!_game.CurrentQuest.CurrentVoteTrack.Players.Any(p => p == _player))
            {
                return _player;
            }

            var playersNotOnTeam = _game.Players.Where(p => p != _player).Except(_game.CurrentQuest.CurrentVoteTrack.Players);
            Player player = null;

            //if I'm evil, put anyone else on
            if (_IAmEvil)
            {
                player = Random(playersNotOnTeam);
                SayTheyAreGood(player.Name);
                return player;
            }

            //if I'm good, try not to put evil on the mission
            player = Random(playersNotOnTeam.Where(p => IKnowTheyAreEvil(p, _game) == false));
            //failing that we need to put someone evil on the mission :(
            if (player == null)
            {
                player = Random(playersNotOnTeam);
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
            var evilCount = CountEvil(_game.CurrentQuest.CurrentVoteTrack.Players, _game);
            if (_IAmEvil)
            {
                if (evilCount >= _game.CurrentQuest.RequiredFails)
                {
                    Say("I like this team");
                    return true;
                }
                Say("This could be ok, but I'm not sure");
                return false;
            }
            else
            {
                if (evilCount >= _game.CurrentQuest.RequiredFails)
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
