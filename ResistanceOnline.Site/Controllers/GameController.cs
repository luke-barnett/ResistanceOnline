using ResistanceOnline.Core;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ResistanceOnline.Site.Controllers
{
	public class GameController : Controller
	{
        static List<Game> _games = new List<Game>();

        public GameController()
        {
            //create a default game to make development easier
            if (_games.Count == 0)
            {
                var game = new Game(6, true);
                game.AddCharacter(Character.LoyalServantOfArthur);
                game.AddCharacter(Character.Assassin);
                game.AddCharacter(Character.LoyalServantOfArthur);
                game.AddCharacter(Character.Percival);
                game.AddCharacter(Character.Morcana);
                game.AddCharacter(Character.Merlin);
                game.JoinGame("Jordan");
                game.JoinGame("Luke");
                game.JoinGame("Jeffrey");
                game.JoinGame("Simon");
                game.JoinGame("Jayvin");
                game.JoinGame("Verne");
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
            var game = new Game(players,impersonationEnabled);

			_games.Add(game);
            game.GameId = _games.IndexOf(game);

            return RedirectToAction("Game", new { gameId = _games.Count  });
        }

        //playerguid can be null for spectators
        public ActionResult Game(int? gameId, Guid? playerGuid)        
        {
            var game = GetGame(gameId);
            if (game == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = new GameViewModel(game, playerGuid);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddCharacter(int gameId, Guid playerGuid, string character) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.AddCharacter((Character)Enum.Parse(typeof(Character), character));
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }        
        [HttpPost]
        public ActionResult PutOnQuest(int gameId, Guid playerGuid, string person) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.PutOnQuest(player, game.Players.First(p => p.Name == person));
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }        
        [HttpPost]
        public ActionResult SubmitQuestCard(int gameId, Guid playerGuid, bool success) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.SubmitQuest(player, success);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }        
        [HttpPost]
        public ActionResult VoteForQuest(int gameId, Guid playerGuid, bool approve) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.VoteForQuest(player, approve);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }                             
        [HttpPost]
        public ActionResult JoinGame(int gameId, string name) 
        {
            var game = GetGame(gameId);
            var playerGuid = game.JoinGame(name);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }        
        [HttpPost]
        public ActionResult GuessMerlin(int gameId, Guid playerGuid, string guess) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.GuessMerlin(player, game.Players.First(p => p.Name == guess));
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }        

	}
}