﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using ResistanceOnline.Core;
using ResistanceOnline.Database;
using ResistanceOnline.Database.Entities;
using ResistanceOnline.Site.Infrastructure;
using ResistanceOnline.Site.Infrastructure.Messaging;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Action = ResistanceOnline.Core.Action;
using Character = ResistanceOnline.Core.Character;
using Rule = ResistanceOnline.Core.Rule;

namespace ResistanceOnline.Site.Controllers
{
	public class GameHub : Hub
	{
		readonly SimpleDb _simpleDb;
		readonly IMessageService[] _messageServices;

		static Dictionary<string, Guid> _userConnections = new Dictionary<string, Guid>();

		static object _gameCacheLock = new object();
		static Dictionary<int, Game> _gameCache = new Dictionary<int, Game>();

		public GameHub(ResistanceOnlineDbContext context, IMessageService[] messageServices)
		{
			_simpleDb = new SimpleDb(context);
			_messageServices = messageServices;
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
				if (Context != null && Context.User != null)
				{
					var userId = Context.User.Identity.GetUserId();
                    try
                    {
                        userAccount = _simpleDb.GetUser(userId);
                    }
                    catch
                    {
                    }
				}
				if (userAccount == null)
				{
					userAccount = new UserAccount { PlayerGuid = Guid.Empty };
				}
				return userAccount;
			}
		}

		private void Update(bool force = false)
		{
			//loop through all connected players
			foreach (var connection in _userConnections)
			{
				var games = new List<GameModel>();
				foreach (var gameId in _gameCache.Keys)
				{
					var game = _gameCache[gameId];
					var gameModel = new GameModel(gameId, game, connection.Value);

					//only send updates for games with recent actions, or when forced on initial connection (page refresh?)
					if (force || game.LastActionTime > DateTimeOffset.Now.AddDays(-1))
					{
						games.Add(gameModel);
					}
				}

				Clients.Client(connection.Key).Update(games.OrderBy(g => g.GameId).ToList());
			}
		}

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            if (_userConnections.Keys.Contains(Context.ConnectionId))
            {
				_userConnections.Remove(Context.ConnectionId);
            }

            return base.OnDisconnected();
        }

		public override System.Threading.Tasks.Task OnConnected()
		{
			var playerGuid = PlayerGuid;
			_userConnections.Add(Context.ConnectionId, playerGuid);
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

		void SendMessage(string userId, string subject, string message, string gameId)
		{
			foreach(var services in _messageServices)
			{
				services.NotifyPlayerForAttention(userId, subject, message, gameId);
			}
		}

		void SendMessagesAsAppropriate(Game game, int gameId)
		{
			foreach(var player in game.Players.Where(player => player.PlayerType == Player.Type.Human))
			{
                var actions = game.AvailableActions(player).Select(a=>a.ActionType).Where(a => a != Action.Type.Message);
				if(actions.Any() && !_userConnections.ContainsValue(player.Guid))
				{
					var user = _simpleDb.GetUser(player.Guid);

                    SendMessage(
                        user.Id, 
                        ("Your attention is required on " + game.GameName).Humanize(LetterCasing.Sentence), 
                        ("Things have happened and you need to " + Useful.CommaQuibbling(actions.Select(a => a.Humanize()), "or") + ".").Humanize(LetterCasing.Sentence),
                        gameId.ToString()
                    );
				}
			}
		}

		/// <summary>
		/// do an action as the logged on user
		/// </summary>
		private void DoAction(int gameId, Action.Type actionType, string text = null)
		{
			DoAction(gameId, PlayerGuid, actionType, text);
		}

		/// <summary>
		/// do an action as a specific player
		/// </summary>
		private void DoAction(int gameId, Guid owner, Action.Type actionType, string text = null)
		{
			var action = new Action(owner, actionType, text);
			action.GameId = gameId;

			var game = GetGame(gameId);
            var previousState = game.GameState;
			game.DoAction(action);

			_simpleDb.AddAction(action);

			var computersPlayersInGame = game.Players.Where(p => p.PlayerType != Core.Player.Type.Human);
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

            if (game.GameState != previousState)
            {
                SendMessagesAsAppropriate(game, gameId);
            }

			Update();
		}

		public void SetGameName(int gameId, string gamename)
		{
			DoAction(gameId, Action.Type.SetGameName, gamename);
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
			DoAction(gameId, Guid.NewGuid(), Action.Type.AddBot, Useful.RandomName());
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