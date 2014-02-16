using Microsoft.AspNet.Identity;
using ResistanceOnline.Core;
using ResistanceOnline.Database;
using ResistanceOnline.Database.Entities;
using ResistanceOnline.Site.ComputerPlayers;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ResistanceOnline.Site.Controllers
{
	[Authorize]
	public class GameController : Controller
	{
		readonly ResistanceOnlineDbContext _dbContext;
		static List<ComputerPlayer> _computerPlayers = new List<ComputerPlayer>();
		static List<Game> _games = new List<Game>();

		public GameController(ResistanceOnlineDbContext dbContext)
			: this()
		{
			_dbContext = dbContext;
		}

		public GameController()
		{
			//create a default game to make development easier
			if (_games.Count == 0)
			{
				var game = new Game(5);
				game.Rule_LancelotsKnowEachOther = true;
				game.Rule_GoodMustAlwaysVoteSucess = true;
				game.Rule_IncludeLadyOfTheLake = true;
				game.AddCharacter(Character.LoyalServantOfArthur);
				game.AddCharacter(Character.Assassin);
				game.AddCharacter(Character.Percival);
				game.AddCharacter(Character.Morgana);
				game.AddCharacter(Character.Merlin);
				_computerPlayers.Add(new TrustBot(game, game.JoinGame("Jordan", Guid.NewGuid())));
				_computerPlayers.Add(new CheatBot(game, game.JoinGame("Luke", Guid.NewGuid())));
				_computerPlayers.Add(new CheatBot(game, game.JoinGame("Jeffrey", Guid.NewGuid())));
				_computerPlayers.Add(new SimpleBot(game, game.JoinGame("Jayvin", Guid.NewGuid())));

				game.GameId = 0;
				_games.Add(game);
			}
		}

		Game GetGame(int? gameId)
		{
			//todo - something to do with databases
			if (gameId.HasValue == false || gameId.Value >= _games.Count)
				return null;

			return _games[gameId.Value];
		}

		private UserAccount CurrentUser
		{
			get
			{
				var userId = User.Identity.GetUserId();
				return _dbContext.Users.FirstOrDefault(user => user.Id == userId);
			}
		}

		private Guid? PlayerGuid
		{
			get
			{
				return CurrentUser != null ? CurrentUser.PlayerGuid : (Guid?)null;
			}
		}

		[AllowAnonymous]
		public ActionResult Index()
		{
			ViewBag.Games = new List<Game> { };
			ViewBag.Games = _games;
			return View();
		}

		[HttpPost]
		public ActionResult CreateGame(int players)
		{
			//todo - something with the database :)
			var game = new Game(players);

			_games.Add(game);
			game.GameId = _games.IndexOf(game);

			return RedirectToAction("Game", new { gameId = _games.Count });
		}

		[AllowAnonymous]
		public ActionResult Game(int? gameId)
		{
			var game = GetGame(gameId);
			if (game == null)
			{
				return RedirectToAction("Index");
			}

			var viewModel = new GameModel(game, PlayerGuid);

			return View(viewModel);
		}

		[HttpPost]
		public ActionResult AddCharacter(int gameId, string character)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == PlayerGuid);
			game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.AddCharacter, Character = (Character)Enum.Parse(typeof(Character), character) });
			OnAfterAction(game);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		private void OnAfterAction(Game game)
		{
			var state = game.DetermineState();
			if (state == Core.Game.State.Playing || state == Core.Game.State.GuessingMerlin)
			{
				var computersPlayersInGame = _computerPlayers.Where(c => game.Players.Select(p => p.Guid).Contains(c.PlayerGuid));
				while (computersPlayersInGame.Any(c => game.AvailableActions(game.Players.First(p => p.Guid == c.PlayerGuid)).Any(a=>a!= Core.Action.Type.Message)))
				{
					foreach (var computerPlayer in computersPlayersInGame)
					{
						computerPlayer.DoSomething();
					}
				}
			}
		}

		[HttpPost]
		public ActionResult AddToTeam(int gameId, Guid playerGuid, string person)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == playerGuid);
			game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.AddToTeam, Player = game.Players.First(p => p.Name == person) });
			OnAfterAction(game);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult SubmitQuestCard(int gameId, Guid playerGuid, bool success)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == playerGuid);
			game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.SubmitQuestCard, Success = success });
			OnAfterAction(game);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult VoteForTeam(int gameId, Guid playerGuid, bool approve)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == playerGuid);
			game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.VoteForTeam, Accept = approve });
			OnAfterAction(game);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult JoinGame(int gameId)
		{
			var game = GetGame(gameId);
			var playerGuid = game.JoinGame(CurrentUser.UserName, PlayerGuid.Value);
			OnAfterAction(game);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult AddComputerPlayer(int gameId, Guid playerGuid, string bot, string name)
		{
			var game = GetGame(gameId);
			switch (bot)
			{
				case "trustbot":
					_computerPlayers.Add(new ComputerPlayers.TrustBot(game, game.JoinGame(name, Guid.NewGuid())));
					break;
                case "cheatbot":
                    _computerPlayers.Add(new ComputerPlayers.CheatBot(game, game.JoinGame(name, Guid.NewGuid())));
                    break;
                case "simplebot":
				default:
					_computerPlayers.Add(new ComputerPlayers.SimpleBot(game, game.JoinGame(name, Guid.NewGuid())));
					break;
			}
			OnAfterAction(game);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult GuessMerlin(int gameId, Guid playerGuid, string guess)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == playerGuid);
			game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.GuessMerlin, Player = game.Players.First(p => p.Name == guess) });
			OnAfterAction(game);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult LadyOfTheLake(int gameId, Guid playerGuid, string target)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == playerGuid);
			game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.UseTheLadyOfTheLake, Player = game.Players.First(p => p.Name == target) });
			OnAfterAction(game);
			return RedirectToAction("Game", new { gameId = gameId });
		}

        [HttpPost]
        public ActionResult Message(int gameId, Guid playerGuid, string message)
        {
            var game = GetGame(gameId);
            var player = game.Players.FirstOrDefault(p => p.Guid == playerGuid);
            game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.Message, Message = message });
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId });
        }
	}
}