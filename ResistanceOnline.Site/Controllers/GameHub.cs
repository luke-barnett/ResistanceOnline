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
    public class GameHub : Hub
    {
        readonly Infrastructure.SimpleDb _simpleDb;
        
        static Dictionary<Guid, List<string>> _userConnections = new Dictionary<Guid, List<string>>();
        
        static object _gameCacheLock = new object();
        static Dictionary<int, Game> _gameCache = new Dictionary<int, Game>();

        public GameHub()
        {
            _simpleDb = new Infrastructure.SimpleDb(new Database.ResistanceOnlineDbContext());
            InitGameCache();
        }

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
                    userAccount = _simpleDb.GetUser(userId);
                }
                if (userAccount == null)
                {
                    userAccount = new UserAccount { PlayerGuid = Guid.Empty };
                }
                return userAccount;
            }
        }

        private void Update(bool force=false)
        {
            foreach (var guid in _userConnections.Keys)
            {
                var games = new List<GameModel>();
                foreach (var gameId in _gameCache.Keys)
                {
                    var game = _gameCache[gameId];
					var gameModel = new GameModel(gameId, game, guid);

                    //only send updates for games with recent actions, or when forced on initial connection (page refresh?)
                    if (force || game.LastActionTime > DateTimeOffset.Now.AddDays(-1))
                    {
                        games.Add(gameModel);
                    }
                }

                foreach (var connection in _userConnections[guid])
                {
                    Clients.Client(connection).Update(games.OrderBy(g=>g.GameId).ToList());
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

                lock (_gameCacheLock)
                {
                    if (!_gameCache.ContainsKey(0) && PlayerGuid != Guid.Empty)
                    {
                        //create game 0 for development
                        var actions = new List<Action>();
						var guid = PlayerGuid;
						actions.Add(new Action(guid, Action.Type.Join, PlayerName));
						actions.Add(new Action(guid, Action.Type.AddBot, Useful.RandomName()));
						actions.Add(new Action(guid, Action.Type.AddBot, Useful.RandomName()));
						actions.Add(new Action(guid, Action.Type.AddBot, Useful.RandomName()));
						actions.Add(new Action(guid, Action.Type.AddBot, Useful.RandomName()));
						actions.Add(new Action(guid, Action.Type.AddBot, Useful.RandomName()));

						actions.Add(new Action(guid, Action.Type.AddCharacterCard, Character.Merlin.ToString()));
						actions.Add(new Action(guid, Action.Type.AddCharacterCard, Character.Assassin.ToString()));
						actions.Add(new Action(guid, Action.Type.AddCharacterCard, Character.Percival.ToString()));
						actions.Add(new Action(guid, Action.Type.AddCharacterCard, Character.Morgana.ToString()));
						actions.Add(new Action(guid, Action.Type.AddCharacterCard, Character.LoyalServantOfArthur.ToString()));
						actions.Add(new Action(guid, Action.Type.AddCharacterCard, Character.LoyalServantOfArthur.ToString()));

						actions.Add(new Action(guid, Action.Type.AddRule, Rule.LadyOfTheLakeExists.ToString()));

                        var game = new Game(actions);
                        _gameCache.Add(0, game);
                    }
                }
            }
            Update(true);
            return base.OnConnected();
        }

        void InitGameCache()
        {
            lock (_gameCacheLock)
            {
                var gameIds = _simpleDb.GameIds();
                foreach (int gameId in gameIds)
                {
                    if (!_gameCache.ContainsKey(gameId))
                    {
                        var actions = _simpleDb.GetActions(gameId);
                        var game = new Game(new List<Action>());
                        try
                        {
                            actions.ForEach(a => game.DoAction(a));
                        }
                        catch (Exception)
                        {
                            game.GameState = Game.State.Error;
                        }
                        _gameCache.Add(gameId, game);
                    }
                }
            }
        }

        Game GetGame(int gameId)
        {            
            return _gameCache[gameId];
        }

        public void CreateGame()
        {
            if (CurrentUser.PlayerGuid == Guid.Empty)
            {
                return;
            }

            var gameId = _simpleDb.NextGameId();
            _gameCache.Add(gameId, new Game(new List<Action>()));
            DoAction(gameId, Action.Type.Join, CurrentUser.UserName);
        }

        public void DeleteGame(int gameId)
        {
            _simpleDb.DeleteActions(gameId);
            _gameCache.Remove(gameId);
            Update(true);
        }

        private void DoAction(int gameId, Action.Type actionType, string text = null)
        {
            var action = new Action(PlayerGuid, actionType, text);
            action.GameId = gameId;

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
                                computerAction.GameId = gameId;
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

		public void JoinGame(int gameId)
		{
			DoAction(gameId, Action.Type.Join, PlayerName);
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