using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using ResistanceOnline.Core;
using ResistanceOnline.Database;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ResistanceOnline.Site.Controllers
{
	[Authorize]
	public class GameController : Controller
	{
		readonly ResistanceOnlineDbContext _dbContext;

		public GameController(ResistanceOnlineDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		static List<Game> _games = new List<Game>();

		public GameController()
		{
			//create a default game to make development easier
			if (_games.Count == 0)
			{
				var game = new Game(5, true);
				game.AddCharacter(Character.LoyalServantOfArthur);
				game.AddCharacter(Character.Assassin);
				game.AddCharacter(Character.Percival);
				game.AddCharacter(Character.Morgana);
				game.AddCharacter(Character.Merlin);
				var jordanGuid = Guid.NewGuid();
				game.JoinGame("Jordan", jordanGuid);
				var jordan = game.Players.First(player => player.Guid == jordanGuid);
				var lukeGuid = Guid.NewGuid();
				game.JoinGame("Luke", lukeGuid);
				var luke = game.Players.First(player => player.Guid == lukeGuid);
				var jeffreyGuid = Guid.NewGuid();
				game.JoinGame("Jeffrey", jeffreyGuid);
				var jeffrey = game.Players.First(player => player.Guid == jeffreyGuid);
				var jayvinGuid = Guid.NewGuid();
				game.JoinGame("Jayvin", jayvinGuid);
				var jayvin = game.Players.First(player => player.Guid == jayvinGuid);
				var verneGuid = Guid.NewGuid();
				game.JoinGame("Verne", verneGuid);
				var verne = game.Players.First(player => player.Guid == verneGuid);

				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, jordan);
				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, luke);
				game.VoteForTeam(jordan, true);
				game.VoteForTeam(luke, true);
				game.VoteForTeam(jayvin, false);
				game.VoteForTeam(jeffrey, true);
				game.VoteForTeam(verne, false);
				game.SubmitQuest(jordan, true);
				game.SubmitQuest(luke, false);

				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, jordan);
				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, luke);
				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, jayvin);
				game.VoteForTeam(jordan, false);
				game.VoteForTeam(luke, true);
				game.VoteForTeam(jayvin, false);
				game.VoteForTeam(jeffrey, false);
				game.VoteForTeam(verne, false);

				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, jeffrey);
				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, luke);
				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, jayvin);
				game.VoteForTeam(jordan, true);
				game.VoteForTeam(luke, true);
				game.VoteForTeam(jayvin, false);
				game.VoteForTeam(jeffrey, false);
				game.VoteForTeam(verne, true);

				game.SubmitQuest(jeffrey, true);
				game.SubmitQuest(luke, true);
				game.SubmitQuest(jayvin, true);

				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, luke);
				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, jayvin);
				game.VoteForTeam(jordan, true);
				game.VoteForTeam(luke, true);
				game.VoteForTeam(jayvin, false);
				game.VoteForTeam(jeffrey, false);
				game.VoteForTeam(verne, true);

				game.SubmitQuest(luke, true);
				game.SubmitQuest(jayvin, true);

				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, jeffrey);
				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, luke);
				game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, jayvin);
				game.VoteForTeam(jordan, true);
				game.VoteForTeam(luke, true);
				game.VoteForTeam(jayvin, false);
				game.VoteForTeam(jeffrey, false);
				game.VoteForTeam(verne, true);

				game.SubmitQuest(jeffrey, true);
				game.SubmitQuest(luke, true);
				game.SubmitQuest(jayvin, true);

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
		public ActionResult CreateGame(int players, bool impersonationEnabled)
		{
			//todo - something with the database :)
			var game = new Game(players, impersonationEnabled);

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
			game.AddCharacter((Character)Enum.Parse(typeof(Character), character));
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult AddToTeam(int gameId, Guid playerGuid, string person)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == PlayerGuid);
			game.AddToTeam(player, game.Players.First(p => p.Name == person));
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult SubmitQuestCard(int gameId, bool success)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == PlayerGuid);
			game.SubmitQuest(player, success);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult VoteForTeam(int gameId, bool approve)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == PlayerGuid);
			game.VoteForTeam(player, approve);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult JoinGame(int gameId)
		{
			var game = GetGame(gameId);
			game.JoinGame(User.Identity.GetUserName(), PlayerGuid.Value);
			return RedirectToAction("Game", new { gameId = gameId });
		}

		[HttpPost]
		public ActionResult GuessMerlin(int gameId, string guess)
		{
			var game = GetGame(gameId);
			var player = game.Players.First(p => p.Guid == PlayerGuid);
			game.GuessMerlin(player, game.Players.First(p => p.Name == guess));
			return RedirectToAction("Game", new { gameId = gameId });
		}
	}
}