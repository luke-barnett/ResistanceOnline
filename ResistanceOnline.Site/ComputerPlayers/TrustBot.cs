using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Site.ComputerPlayers
{
    public class TrustBot : ComputerPlayer
    {        
        public TrustBot(Game game, Guid playerGuid) : base(game, playerGuid) { }

        private double DetermineTrustLevel(Player player) 
        {

            if (ConfirmedGood(player))
            {
                return 1;
            }
            if (ConfirmedEvil(player))
            {
                return 0;
            }

            double trust = 0;
            var evilCharactersInGame = _game.AvailableCharacters.Count(c => _game.IsCharacterEvil(c));
            trust = (double)evilCharactersInGame / (double)_game.GameSize;

            //nothing confirmed, look at quest behaviour
            foreach (var round in _game.Rounds)
            {
                var onTeam = round.Teams.Last().TeamMembers.Contains(player);
                if (onTeam)
                {
                    var fails = round.Teams.Last().Quests.Count(q => !q.Success);
                    var size = round.Teams.Last().TeamMembers.Count();

                    int evilProbability = (int)(((double)fails / (double)size) * 100.0);
                    if (trust < evilProbability)
                    {
                        trust = evilProbability;
                    }
                }
            }

            return trust;
        }    

        private bool ConfirmedEvil(Player player)
        {
            var knowledge = _game.PlayerKnowledge(_player, player);
            if (knowledge == Knowledge.Evil || (knowledge == Knowledge.EvilLancelot && !_game.LancelotAllegianceSwitched) || (knowledge == Knowledge.Lancelot && _game.LancelotAllegianceSwitched))
            {
                return true;
            }
            return false;
        }

        private bool ConfirmedGood(Player player)
        {
            var knowledge = _game.PlayerKnowledge(_player, player);
            if (knowledge == Knowledge.Good || (knowledge == Knowledge.EvilLancelot && _game.LancelotAllegianceSwitched) || (knowledge == Knowledge.Lancelot && !_game.LancelotAllegianceSwitched))
            {
                return true;
            }
            return false;
        }

        protected override Core.Player LadyOfTheLakeTarget()
        {
            var eligiblePlayers = _game.Players.Where(p => p.Guid != PlayerGuid).Except(_game.LadyOfTheLakeUses.Select(u => u.UsedBy));

            //use it on the person you know the least about
            return eligiblePlayers.Select(p => new { Player = p, Trust = Math.Abs(DetermineTrustLevel(p)) }).OrderBy(p => p.Trust).Select(p=>p.Player).First();
            
        }

        protected override Core.Player GuessMerlin()
        {
            //suspect the most good person
            return _game.Players.Where(p => p.Guid != PlayerGuid).Select(p => new { Player = p, Trust = DetermineTrustLevel(p) }).OrderByDescending(p => p.Trust).Select(p => p.Player).First();
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

            //if I'm good, put most trustworthy person on
            return playersNotOnTeam.Select(p => new { Player = p, Trust = DetermineTrustLevel(p) }).OrderByDescending(p => p.Trust).Select(p => p.Player).First();
        }

        protected override bool Quest()
        {
            return _IAmEvil == false;
        }

        protected override bool TeamVote()
        {
            //always succeed the last round
            if (_game.CurrentRound.Teams.Count == 5)
            {
                return true;
            }            

            //work out how many evil players I think might be on the team
            var evilCount = _game.CurrentRound.CurrentTeam.TeamMembers.Select(p => IsProbablyEvil(p)).Count(x => x);
            if (_IAmEvil)
            {
                if (evilCount >= _game.CurrentRound.RequiredFails)
                    return true;
                return false;
            }
            else
            {
                if (evilCount >= _game.CurrentRound.RequiredFails)
                    return false;
                return true;
            }

        }

        private bool IsProbablyEvil(Player player)
        {
            var trust = DetermineTrustLevel(player);
            return (new Random().Next(100) > trust * 100);            
        }
     
    }
        
}
