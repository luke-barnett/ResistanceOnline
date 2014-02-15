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

    }
}
