using Microsoft.AspNet.SignalR;
using ResistanceOnline.Core;
using ResistanceOnline.Database;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Web.Mvc;
using ResistanceOnline.Database.Entities;
using Action = ResistanceOnline.Core.Action;
using Rule = ResistanceOnline.Core.Rule;
using Character = ResistanceOnline.Core.Character;


namespace ResistanceOnline.Site.Controllers
{
    //[Authorize]
    public class GameHub : Hub
    {
        readonly ResistanceOnlineDbContext _dbContext;
        Infrastructure.SimpleDb _simpleDb;
        public GameHub()//(ResistanceOnlineDbContext dbContext)
        {
            _dbContext = new Database.ResistanceOnlineDbContext(); //todo injection
            _simpleDb = new Infrastructure.SimpleDb(_dbContext);
        }

        //todo logged in user
        private Guid PlayerGuid
        {
            get
            {
                return CurrentUser.PlayerGuid;
            }
        }

        private UserAccount CurrentUser
        {
            get
            {
                UserAccount userAccount = null;
                if (Context.User != null)
                {
                    var userId = Context.User.Identity.GetUserId();
                    userAccount = _dbContext.Users.FirstOrDefault(user => user.Id == userId);
                }
                if (userAccount == null)
                {
                    userAccount = new UserAccount { PlayerGuid = Guid.Empty };
                }
                return userAccount;
            }
        }


        static Dictionary<Guid, List<string>> _userConnections = new Dictionary<Guid, List<string>>();



        private Game GetGame(int? gameId)
        {
            if (gameId.HasValue == false)
                return null;

            return _simpleDb.GetGame(gameId.Value);
        }


        private void Update()
        {
            foreach (var guid in _userConnections.Keys)
            {
                //todo - don't need all games sent every update
                //it feels like this should be split out into game hubs for playing games and home page stuff for managing games
                //var games = null;// _dbContext.Games.ToList().Select(g => new GameModel(GetGame(g.GameId), guid));

                foreach (var connection in _userConnections[guid])
                {
                    Clients.Client(connection).Update(null);
                }
            }
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            //todo cleanup on disconnection
            if (PlayerGuid != Guid.Empty)
            {
                if (!_userConnections.ContainsKey(PlayerGuid))
                {
                    _userConnections[PlayerGuid] = new List<string>();
                }
                _userConnections[PlayerGuid].Add(Context.ConnectionId);
            }
            Update();
            return base.OnConnected();
        }

        public Game CreateGame()
        {
            var game = _simpleDb.CreateGame(PlayerGuid, CurrentUser.UserName);
            Update();
            return game;
        }

        private void DoAction(int gameId, Action.Type actionType, string targetPlayerName = null, string text = null)
        {
            var game = GetGame(gameId);
            var action = new Action(PlayerGuid, actionType, text);
            game.DoAction(action);
            _simpleDb.AddAction(action);
            LetComputerPlayersDoActions(game);
        }

        private void LetComputerPlayersDoActions(Game game)
        {
            var state = game.GameState;
            var computersPlayersInGame = game.Players.Where(p=>p.PlayerType != Core.Player.Type.Human);
            while (computersPlayersInGame.Any(c => game.AvailableActions(game.Players.First(p => p.Guid == c.Guid)).Any(action => action != Action.Type.Message)))
            {
                foreach (var computerPlayer in computersPlayersInGame)
                {                    
                    var action = Core.ComputerPlayers.ComputerPlayer.Factory(computerPlayer.PlayerType, computerPlayer.Guid).DoSomething(game);
                    if (action != null)
                    {
                        game.DoAction(action);
                        _simpleDb.AddAction(action);
                    }
                }
            }
        }

        public void AddToTeam(int gameId, string person)
        {
            DoAction(gameId, Action.Type.AddToTeam, targetPlayerName: person);
            Update();
        }

        public void SucceedQuest(int gameId)
        {
            DoAction(gameId, Action.Type.SucceedQuest);
            Update();
        }

        public void FailQuest(int gameId)
        {
            DoAction(gameId, Action.Type.FailQuest);
            Update();
        }

        public void VoteApprove(int gameId)
        {
            DoAction(gameId, Action.Type.VoteApprove);
            Update();
        }

        public void VoteReject(int gameId)
        {
            DoAction(gameId, Action.Type.VoteReject);
            Update();
        }

        public void GuessMerlin(int gameId, string guess)
        {
            DoAction(gameId, Action.Type.GuessMerlin, targetPlayerName:guess);
            Update();
        }

        public void LadyOfTheLake(int gameId, string target)
        {
            DoAction(gameId, Action.Type.UseTheLadyOfTheLake, targetPlayerName: target);
            Update();
        }


        public void Message(int gameId, string message)
        {
            DoAction(gameId, Action.Type.Message, text: message);
            Update();
        }

        public void AssignExcalibur(int gameId, string proposedPlayerName)
        {
            DoAction(gameId, Action.Type.AssignExcalibur, targetPlayerName:proposedPlayerName);
            Update();
        }

        public void UseExcalibur(int gameId, string proposedPlayerName)
        {
            DoAction(gameId, Action.Type.UseExcalibur, targetPlayerName: proposedPlayerName);
            Update();
        }
    }
}