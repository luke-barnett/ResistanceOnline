using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = ResistanceOnline.Core.Action;

namespace ResistanceOnline.Site.ComputerPlayers
{
    public abstract class ComputerPlayer
    {
        protected bool _IAmEvil;
        protected GamePlay _gameplay;
        protected Player _player;
        public Guid PlayerGuid { get; private set; }
        private List<string> _thingsIWantToSay = new List<string>();
        
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

        public Action DoSomething(GamePlay gameplay) 
        {
            _gameplay = gameplay;
            _IAmEvil = _gameplay.Game.IsCharacterEvil(_player.Character, false);

            var availableActions = _gameplay.AvailableActions(_player);
            if (availableActions.Count == 0)
                return null;

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.Message) && _thingsIWantToSay.Count > 0)
            {
                var message = _thingsIWantToSay[0];
                _thingsIWantToSay.RemoveAt(0);

                return new Action(_player, Action.Type.Message, text:message);
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.VoteApprove))
            {
                return new Action(_player, TeamVote() ? Action.Type.VoteApprove : Action.Type.VoteReject);
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.AssignExcalibur))
            {
                return new Action(_player, Action.Type.AssignExcalibur, AssignExcalibur());
            }


            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.UseExcalibur))
            {
                return new Action(_player, Action.Type.UseExcalibur, UseExcalibur());
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.FailQuest) || availableActions.Contains(ResistanceOnline.Core.Action.Type.SucceedQuest))
            {
                return new Action(_player, Quest() ? Action.Type.SucceedQuest : Action.Type.FailQuest);
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.AddToTeam))
            {
                return new Action(_player, Action.Type.AddToTeam, ChooseTeamPlayer());
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.GuessMerlin))
            {
                return new Action(_player, Action.Type.GuessMerlin, GuessMerlin());
            }

            if (availableActions.Contains(ResistanceOnline.Core.Action.Type.UseTheLadyOfTheLake))
            {
                return new Action(_player, Action.Type.UseTheLadyOfTheLake, LadyOfTheLakeTarget());
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
            return _gameplay.CurrentRound.CurrentTeam.TeamMembers.RandomOrDefault(p => p.Name != _player.Name);            
        }        
    }
}
