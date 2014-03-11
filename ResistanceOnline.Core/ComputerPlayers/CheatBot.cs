using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core.ComputerPlayers
{
    public class CheatBot : ComputerPlayer
    {
        public CheatBot(Guid playerGuid) : base(playerGuid) { }
        
        protected override Core.Player LadyOfTheLakeTarget()
        {
            var ladyOfTheLakeHistory = _gameplay.Quests.Where(r => r.LadyOfTheLake != null).Select(r => r.LadyOfTheLake.Holder);
            return _gameplay.Game.Players.Where(p => p != _player).Except(ladyOfTheLakeHistory).Random();
        }

        protected override Core.Player GuessMerlin()
        {
            Say("Hahaha, I saw who merlin was at the beginning of the game!");
            return _gameplay.Game.Players.FirstOrDefault(p => p.Character == Character.Merlin);
        }

        protected override Core.Player ChooseTeamPlayer()
        {
            //put myself on
            if (!_gameplay.CurrentQuest.CurrentVoteTrack.Players.Any(p => p == _player))
            {
                return _player;
            }

            var playersNotOnTeam = _gameplay.Game.Players.Where(p => p != _player).Except(_gameplay.CurrentQuest.CurrentVoteTrack.Players);
            Player player = null;

            //if I'm evil, put anyone else on
            if (_IAmEvil)
            {
                player = playersNotOnTeam.Random();
                SayTheyAreGood(player.Name);
                return player;
            }

            //if I'm good, only put good on
            player = playersNotOnTeam.RandomOrDefault(p => !_gameplay.Game.IsCharacterEvil(p.Character, false));
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
            var evilCount = _gameplay.CurrentQuest.CurrentVoteTrack.Players.Count(p => _gameplay.Game.IsCharacterEvil(p.Character, false));

            if (_IAmEvil)
            {
                if (evilCount >= _gameplay.CurrentQuest.RequiredFails)
                {
                    SayTeamIsOk();
                    return true;
                }
                SayTeamNotOk();
                return false;
            }
            else
            {
                if (evilCount >= _gameplay.CurrentQuest.RequiredFails)
                {
                    SayTeamNotOk();
                    return false;
                }
                SayTeamIsOk();
                return true;
            }

        }

        protected override Player UseExcalibur()
        {
            var quest = _gameplay.CurrentQuest.CurrentVoteTrack.QuestCards.FirstOrDefault(i => i.Success != _IAmEvil);
            return quest == null ? null : quest.Player;
        }
    }
        
}
