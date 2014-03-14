using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = ResistanceOnline.Core.Action;

namespace ResistanceOnline.Core.ComputerPlayers
{
    public abstract class ComputerPlayer
    {
        protected bool _IAmEvil;
        protected Game _game;
        protected Player _player;
        public Guid PlayerGuid { get; private set; }
        private List<string> _thingsIWantToSay = new List<string>();

        protected Player Random(IEnumerable<Player> players)
        {
            return players.OrderBy(p => Guid.NewGuid()).First();
        }

        public static ComputerPlayer Factory(Player.Type type, Guid playerGuid)
        {
            switch (type)
            {
                case Player.Type.TrustBot:
                default:
                    return new TrustBot(playerGuid);
            }
        }

        public ComputerPlayer(Guid playerGuid)
        {
            PlayerGuid = playerGuid;
        }

        public void Say(string message)
        {
            _thingsIWantToSay.Add(message);
        }

        private List<string> _goodThingsToSay = new List<string>() 
        {
            "I trust {0}",
            "{0} is totally good",
            "{0} is good",
            "Hey everyone, {0} is good",
            "{0} is Merlin",
            "If you believe I'm good (which I am) then you should believe {0} is good too",
            "I can't tell you how but I know {0} is good",
            "I'm good and I know {0} is good",
            "I'm evil, but {0} is good",
            "Me and {0} are both good",
            "I'm a cheatbot so you know it's true when I say {0} is good"
        };
        private List<string> _badThingsToSay = new List<string>() 
        {
            "I don't trust {0}",
            "{0} is totally evil",
            "You really need to trust me, {0} is evil",
            "Don't trust {0}, they are evil",
            "Hey everyone, {0} is evil",
            "{0} is Morgana",
            "I can't tell you how, but I know {0} is evil",
            "If you think I'm evil then you think {0} is evil",
            "I'm a cheatbot so you know it's true when I say {0} is evil"
        };

        private List<string> _teamIsOk = new List<string>() 
        {
            "I like this team",
            "This team will succeed",
            "Looks good to me",
            "If you are good you have to accept this team",
            "Everyone needs to trust me when I say this is a good team",
            "I'm a cheatbot and I know this team is successful",
            "Awesome",
            "I'm very happy with this",
            "Sweet. Keep this team next time"
        };

        private List<string> _teamIsNotOk = new List<string>() 
        {
            "I really don't like this team",
            "This team will not succeed",
            "DO NOT ACCEPT THIS TEAM!",
            "If you are good then you have to reject this team",
            "Everyone needs to trust me when I say this is not a good team",
            "I'm a cheatbot and I know this is not a good team",
            "OMG this is bad",
            "We're screwed now"
        };

        public void SayTheyAreGood(string player)
        {
            Say(String.Format(_goodThingsToSay.OrderBy(x=>Guid.NewGuid()).First(), player));
        }

        public void SayTheyAreEvil(string player)
        {
            Say(String.Format(_badThingsToSay.OrderBy(x => Guid.NewGuid()).First(), player));
        }

        public void SayTeamIsOk()
        {
            Say(_teamIsOk.OrderBy(x => Guid.NewGuid()).First());
        }

        public void SayTeamNotOk()
        {
            Say(_teamIsNotOk.OrderBy(x=>Guid.NewGuid()).First());
        }

        public Action DoSomething(Game game) 
        {
            _game = game;
            _player = _game.Players.First(p => p.Guid == PlayerGuid);
            _IAmEvil = _game.IsCharacterEvil(_player.Character, false);

            var availableActions = _game.AvailableActions(_player);
            if (availableActions.Count == 0)
                return null;

            if (availableActions.Any(a=>a.Action==ResistanceOnline.Core.Action.Type.Message) && _thingsIWantToSay.Count > 0)
            {
                var message = _thingsIWantToSay[0];
                _thingsIWantToSay.RemoveAt(0);

                return new Action(PlayerGuid, Action.Type.Message, text: message);
            }

            if (availableActions.Any(a=>a.Action==ResistanceOnline.Core.Action.Type.VoteApprove))
            {
                return new Action(PlayerGuid, TeamVote() ? Action.Type.VoteApprove : Action.Type.VoteReject);
            }

            if (availableActions.Any(a => a.Action == ResistanceOnline.Core.Action.Type.AssignExcalibur))
            {
                return new Action(PlayerGuid, Action.Type.AssignExcalibur, AssignExcalibur().Name);
            }


            if (availableActions.Any(a => a.Action == ResistanceOnline.Core.Action.Type.UseExcalibur))
            {
                return new Action(PlayerGuid, Action.Type.UseExcalibur, UseExcalibur().Name);
            }

            if (availableActions.Any(a => a.Action == ResistanceOnline.Core.Action.Type.FailQuest) || availableActions.Any(a => a.Action == ResistanceOnline.Core.Action.Type.SucceedQuest))
            {
                return new Action(PlayerGuid, Quest() ? Action.Type.SucceedQuest : Action.Type.FailQuest);
            }

            if (availableActions.Any(a => a.Action == ResistanceOnline.Core.Action.Type.AddToTeam))
            {
                return new Action(PlayerGuid, Action.Type.AddToTeam, ChooseTeamPlayer().Name);
            }

            if (availableActions.Any(a => a.Action == ResistanceOnline.Core.Action.Type.GuessMerlin))
            {
                return new Action(PlayerGuid, Action.Type.GuessMerlin, GuessMerlin().Name);
            }

            if (availableActions.Any(a => a.Action == ResistanceOnline.Core.Action.Type.UseTheLadyOfTheLake))
            {
                return new Action(PlayerGuid, Action.Type.UseTheLadyOfTheLake, LadyOfTheLakeTarget().Name);
            }

            return null;
        }

        protected abstract Player LadyOfTheLakeTarget();

        protected abstract Player GuessMerlin();

        protected abstract Player ChooseTeamPlayer();

        protected abstract bool Quest();

        protected abstract bool TeamVote();

        protected abstract Player UseExcalibur();

        //todo implement for each bot
        protected virtual Player AssignExcalibur()
        {
            return Random(_game.CurrentQuest.CurrentVoteTrack.Players.Where(p => p.Name != _player.Name));            
        }        
    }
}
