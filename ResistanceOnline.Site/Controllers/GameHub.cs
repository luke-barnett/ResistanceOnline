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
using ResistanceOnline.Site.Infrastructure;


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
        private string PlayerName
        {
            get
            {
                return CurrentUser.UserName;
            }
        }

        private UserAccount CurrentUser
        {
            get
            {
                UserAccount userAccount = null;
                if (Context!= null && Context.User != null)
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

        private void Update(bool force=false)
        {
            foreach (var guid in _userConnections.Keys)
            {
                var games = new List<GameModel>();
                foreach (var gameId in _gameCache.Keys)
                {
                    var game = _gameCache[gameId];
                    var gameModel = new GameModel(gameId, _gameCache[gameId], PlayerGuid);

                    //only send updates for games with recent actions, or when forced on initial connection (page refresh?)
                    if (force || game.LastActionTime > DateTimeOffset.Now.AddDays(-1))
                    {
                        games.Add(gameModel);
                    }
                }

                foreach (var connection in _userConnections[guid])
                {
                    Clients.Client(connection).Update(games);
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

                if (!_gameCache.ContainsKey(0) && PlayerGuid != Guid.Empty)
                {
                    //create game 0 for development
                    var actions = new List<Action>();
                    actions.Add(new Action(PlayerGuid, Action.Type.Join, PlayerName));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddBot, "Alice"));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddBot, "Bob"));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddBot, "Chuck"));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddBot, "Dan"));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddBot, "Eve"));

                    actions.Add(new Action(PlayerGuid, Action.Type.AddCharacterCard, Character.Merlin.ToString()));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddCharacterCard, Character.Assassin.ToString()));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddCharacterCard, Character.Percival.ToString()));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddCharacterCard, Character.Morgana.ToString()));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddCharacterCard, Character.LoyalServantOfArthur.ToString()));
                    actions.Add(new Action(PlayerGuid, Action.Type.AddCharacterCard, Character.LoyalServantOfArthur.ToString()));

                    actions.Add(new Action(PlayerGuid, Action.Type.AddRule, Rule.LadyOfTheLakeExists.ToString()));
                    //actions.Add(new Action(PlayerGuid, Action.Type.Start, "0"));

                    var game = new Game(actions);
                    _gameCache.Add(0, game);
                }
            }
            Update(true);
            return base.OnConnected();
        }

        private static Dictionary<int, Game> _gameCache = new Dictionary<int, Game>();
        Game GetGame(int gameId)
        {
            if (!_gameCache.ContainsKey(gameId))
            {
                var actions = _simpleDb.GetActions(gameId);
                var game = new Game(actions);
                _gameCache.Add(gameId, game);
            }
            return _gameCache[gameId];
        }

        public void CreateGame()
        {
            var gameId = _simpleDb.NextGameId();
            DoAction(gameId, Action.Type.Join, CurrentUser.UserName);
        }

        private void DoAction(int gameId, Action.Type actionType, string text = null)
        {
            var action = new Action(PlayerGuid, actionType, text);

            var game = GetGame(gameId);
            game.DoAction(action);
            _simpleDb.AddAction(action);

            var computersPlayersInGame = game.Players.Where(p=>p.PlayerType != Core.Player.Type.Human);
            if (game.GameState != Game.State.Lobby)
            {
                while (computersPlayersInGame.Any(c => game.AvailableActions(game.Players.First(p => p.Guid == c.Guid)).Any(a => a.ActionType != Action.Type.Message)))
                {
                    foreach (var computerPlayer in computersPlayersInGame)
                    {
                        var computerActions = Core.ComputerPlayers.ComputerPlayer.Factory(computerPlayer.PlayerType, computerPlayer.Guid).DoSomething(game);
                        if (computerActions != null)
                        {
                            foreach (var computerAction in computerActions)
                            {
                                game.DoAction(computerAction);
                                _simpleDb.AddAction(computerAction);
                            }
                        }
                    }
                }
            }

            Update();
        }

        public void AddToTeam(int gameId, string person)
        {
            DoAction(gameId, Action.Type.AddToTeam, person);
        }

        public void RemoveFromTeam(int gameId, string person)
        {
            DoAction(gameId, Action.Type.RemoveFromTeam, person);
        }

        public void AddBot(int gameId)
        {
            DoAction(gameId, Action.Type.AddBot, Useful.RandomName());
        }

        public void AddCharacterCard(int gameId, string card)
        {
            DoAction(gameId, Action.Type.AddCharacterCard, card);
        }

        public void AddRule(int gameId, string rule)
        {
            DoAction(gameId, Action.Type.AddRule, rule);
        }

        public void RemoveCharacterCard(int gameId, string card)
        {
            DoAction(gameId, Action.Type.RemoveCharacterCard, card);
            Update();
        }

        public void RemoveRule(int gameId, string rule)
        {
            DoAction(gameId, Action.Type.RemoveRule, rule);
            Update();
        }

        public void StartGame(int gameId)
        {
            DoAction(gameId, Action.Type.Start, new Random().Next(int.MaxValue).ToString());
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
            DoAction(gameId, Action.Type.GuessMerlin, guess);
            Update();
        }

        public void LadyOfTheLake(int gameId, string target)
        {
            DoAction(gameId, Action.Type.UseTheLadyOfTheLake, target);
            Update();
        }


        public void Message(int gameId, string message)
        {
            DoAction(gameId, Action.Type.Message, text: message);
            Update();
        }

        public void AssignExcalibur(int gameId, string proposedPlayerName)
        {
            DoAction(gameId, Action.Type.AssignExcalibur, proposedPlayerName);
            Update();
        }

        public void UseExcalibur(int gameId, string proposedPlayerName)
        {
            DoAction(gameId, Action.Type.UseExcalibur, proposedPlayerName);
            Update();
        }
    }
}