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

        private double ProbabilityOfEvil(Player player)
        {
            var knowledge = _game.PlayerKnowledge(_player, player);
            if (knowledge == Knowledge.Evil || (knowledge == Knowledge.EvilLancelot && !_game.LancelotAllegianceSwitched) || (knowledge == Knowledge.Lancelot && _game.LancelotAllegianceSwitched))
            {
                return 100;
            }

            if (knowledge == Knowledge.Good || (knowledge == Knowledge.EvilLancelot && _game.LancelotAllegianceSwitched) || (knowledge == Knowledge.Lancelot && !_game.LancelotAllegianceSwitched))
            {
                return 0;
            }

            double evilProbability = 0;

            var evilCharactersInGame = _game.AvailableCharacters.Count(c => _game.IsCharacterEvil(c));
            if (_IAmEvil)
            {
                evilCharactersInGame--;
            }

            evilProbability = (double)evilCharactersInGame / (double)(_game.GameSize - 1);

            int correctVotes = 0, votesCounted = 0;

            //nothing confirmed, look at quest behaviour
            foreach (var round in _game.Rounds)
            {
                var onTeam = round.Teams.Last().TeamMembers.Contains(player);
                if (onTeam)
                {
                    var fails = round.Teams.Last().Quests.Count(q => !q.Success.Value);
                    var size = round.Teams.Last().TeamMembers.Count();

                    if (round.Teams.Last().TeamMembers.Contains(_player))
                    {
                        if (_IAmEvil) { fails = fails - 1; }
                        size = size - 1;
                    }

                    int roundEvilProbability = (int)(((double)fails / (double)size) * 100.0);
                    if (roundEvilProbability > evilProbability)
                    {
                        evilProbability = roundEvilProbability;
                    }
                }

                if(round.Teams.Count() < 5) { //ignore last round as everyone votes accept                    
                    if (round.RoundState == Round.State.Finished)
                    {
                        var vote = round.Teams.Last().Votes.FirstOrDefault(v => v.Player == player);
                        if (vote.Approve == round.IsSuccess.Value)
                            correctVotes++;
                        votesCounted++;
                    }
                }
            }

            //if they vote correctly each round they're probably merlin.            
            //todo handle off by a couple
            //also check known evils instead of just outcome
            if (correctVotes == votesCounted && votesCounted > 1)
            {
                evilProbability = 0;
            }

            return evilProbability;
        }



        protected override Core.Player LadyOfTheLakeTarget()
        {
            var ladyOfTheLakeHistory = _game.Rounds.Where(r => r.LadyOfTheLake != null).Select(r => r.LadyOfTheLake.Holder);
            var eligiblePlayers = _game.Players.Where(p => p.Guid != PlayerGuid).Except(ladyOfTheLakeHistory);

            //use it on the person you know the least about
            return eligiblePlayers.Select(p => new { Player = p, Confidence = Math.Abs(ProbabilityOfEvil(p) - 0.5) }).OrderBy(p => p.Confidence).Select(p => p.Player).First();

        }

        protected override Core.Player GuessMerlin()
        {
            //suspect the most good person
            return _game.Players.Where(p => p.Guid != PlayerGuid).Select(p => new { Player = p, ProbabilityOfEvil = ProbabilityOfEvil(p) }).OrderBy(p => p.ProbabilityOfEvil).Select(p => p.Player).First();
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
            Player player = null;
            if (_IAmEvil)
            {
                player = playersNotOnTeam.Random();
                SayTheyAreGood(player.Name);
                return player;
            }

            //if I'm good, put most trustworthy person on
            player = playersNotOnTeam.Select(p => new { Player = p, ProbabilityOfEvil = ProbabilityOfEvil(p) }).OrderBy(p => p.ProbabilityOfEvil).Select(p => p.Player).First();
            SayTheyAreGood(player.Name);
            return player;
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
                SayTeamIsOk();
                return true;
            }

            //work out how many evil players I think might be on the team
            var evilCount = _game.CurrentRound.CurrentTeam.TeamMembers.Select(p => IsProbablyEvil(p)).Count(x => x);
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

        private bool IsProbablyEvil(Player player)
        {
            var trust = ProbabilityOfEvil(player);
            return (new Random().Next(100) < trust);
        }


        protected override Player UseExcalibur()
        {
            var teamMembers = _game.CurrentRound.CurrentTeam.TeamMembers;

            foreach(var player in teamMembers) {
                if(ProbabilityOfEvil(player) == (_IAmEvil ? 0 : 100)) {
                    return player;
                }
            }

            return null;
        }
    }

}
