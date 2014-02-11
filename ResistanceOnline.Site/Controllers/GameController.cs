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
                var game = new Game(5);
                game.Rule_PlayersCanImpersonateOtherPlayers = true;
                game.Rule_LancelotsKnowEachOther = true;
                game.Rule_GoodMustAlwaysVoteSucess = true;
                game.Rule_IncludeLadyOfTheLake = true;
                game.AddCharacter(Character.LoyalServantOfArthur);
                game.AddCharacter(Character.Assassin);
                game.AddCharacter(Character.Percival);
                game.AddCharacter(Character.Morgana);
                game.AddCharacter(Character.Merlin);
                var jordanGuid = game.JoinGame("Jordan");
                var jordan = game.Players.First(p=>p.Guid == jordanGuid);
                var lukeGuid = game.JoinGame("Luke");
                var luke = game.Players.First(p=>p.Guid == lukeGuid);
                var jeffreyGuid = game.JoinGame("Jeffrey");
                var jeffrey = game.Players.First(p=>p.Guid == jeffreyGuid);
                var jayvinGuid = game.JoinGame("Jayvin");
                var jayvin = game.Players.First(p=>p.Guid == jayvinGuid);
                var verneGuid = game.JoinGame("Verne");
                var verne = game.Players.First(p=>p.Guid == verneGuid);                

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
        public ActionResult CreateGame(int players)
        {
            //todo - something with the database :)
            var game = new Game(players);

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

            var viewModel = new GameModel(game, playerGuid);

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
        public ActionResult AddToTeam(int gameId, Guid playerGuid, string person) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.AddToTeam(player, game.Players.First(p => p.Name == person));
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
        public ActionResult VoteForTeam(int gameId, Guid playerGuid, bool approve) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.VoteForTeam(player, approve);
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
        [HttpPost]
        public ActionResult LadyOfTheLake(int gameId, Guid playerGuid, string target)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.UseLadyOfTheLake(player, game.Players.First(p => p.Name == target));
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }
	}
}