using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Site.ComputerPlayers
{
    public abstract class ComputerPlayer
    {
        protected bool _IAmEvil;
        protected Game _game;
        protected Player _player;
        public Guid PlayerGuid { get; private set; }
        
        public ComputerPlayer(Game game, Guid playerGuid)
        {
            PlayerGuid = playerGuid;
            _player = game.Players.First(p=>p.Guid == playerGuid);
            _game = game;
        }

        public void Say(string message)
        {
            _game.PerformAction(_player, new Core.Action { ActionType = Core.Action.Type.Message, Message = message });
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
            Say(String.Format(_goodThingsToSay.Random(), player));
        }

        public void SayTheyAreEvil(string player)
        {
            Say(String.Format(_badThingsToSay.Random(), player));
        }

        public void SayTeamIsOk()
        {
            Say(_teamIsOk.Random());
        }

        public void SayTeamNotOk()
        {
            Say(_teamIsNotOk.Random());
        }

        public void DoSomething() 
        {
            _IAmEvil = _game.IsCharacterEvil(_player.Character);

            var availableActions = _game.AvailableActions(_player);
            if (availableActions.Count() == 0)
                return;

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.VoteForTeam))
            {
                _game.PerformAction(_player, new ResistanceOnline.Core.Action { ActionType = ResistanceOnline.Core.Action.Type.VoteForTeam, Accept = TeamVote() });
                return;
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.AssignExcalibur))
            {
                _game.PerformAction(_player, new ResistanceOnline.Core.Action { ActionType = ResistanceOnline.Core.Action.Type.AssignExcalibur, Player = AssignExcalibur() });
                return;
            }


            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.UseExcalibur))
            {
                _game.PerformAction(_player, new ResistanceOnline.Core.Action { ActionType = ResistanceOnline.Core.Action.Type.UseExcalibur, Player = UseExcalibur() });
                return;
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.SubmitQuestCard))
            {
                _game.PerformAction(_player, new ResistanceOnline.Core.Action { ActionType = ResistanceOnline.Core.Action.Type.SubmitQuestCard, Success = Quest() });
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.AddToTeam))
            {
                _game.PerformAction(_player, new ResistanceOnline.Core.Action { ActionType = ResistanceOnline.Core.Action.Type.AddToTeam, Player = ChooseTeamPlayer() });
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.GuessMerlin))
            {
                _game.PerformAction(_player, new ResistanceOnline.Core.Action { ActionType = ResistanceOnline.Core.Action.Type.GuessMerlin, Player = GuessMerlin() });
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.UseTheLadyOfTheLake))
            {
                _game.PerformAction(_player, new ResistanceOnline.Core.Action { ActionType = ResistanceOnline.Core.Action.Type.UseTheLadyOfTheLake, Player = LadyOfTheLakeTarget() });
            }
        }

        protected abstract Player LadyOfTheLakeTarget();

        protected abstract Player GuessMerlin();

        protected abstract Player ChooseTeamPlayer();

        protected abstract bool Quest();

        protected abstract bool TeamVote();

        //todo implement for each bot
        protected virtual Player AssignExcalibur()
        {
            return _game.CurrentRound.CurrentTeam.TeamMembers.RandomOrDefault(p => p != _player);            
        }


        //todo implement for each bot
        protected virtual Player UseExcalibur()
        {
            //don't use excalibur currently
            return null;
        }
    }
}
