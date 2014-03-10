using Humanizer;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using ResistanceOnline.Core;
using ResistanceOnline.Database;
using ResistanceOnline.Database.Entities;
using ResistanceOnline.Site.ComputerPlayers;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Game = ResistanceOnline.Core.Game;

namespace ResistanceOnline.Site.Controllers
{
	[Authorize]
	public class GameController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

        private Game GetGame(int gameId)
        {
            return GameHub.Games.Single(s => s.GameId == gameId);
        }

        public ActionResult Game(int gameId)
        {
            var game = GetGame(gameId);
            ViewBag.IsPlayer = false;
            ViewBag.CanUpdateGame = false;
            ViewBag.CanStartGame = false;

            ViewBag.AllCharactersSelectList =
                Enum.GetValues(typeof(Character))
                    .Cast<Character>()
                    .Select(c => new SelectListItem { Text = c.Humanize(LetterCasing.Sentence), Value = c.ToString() })
                    .ToList();

            ViewBag.AllRulesSelectList =
                Enum.GetValues(typeof(Rule))
                    .Cast<Rule>()
                    .Select(r => new SelectListItem { Text = r.Humanize(LetterCasing.Sentence), Value = r.ToString() })
                    .ToList();

            var userId = User.Identity.GetUserId();
            using (var context = new Database.ResistanceOnlineDbContext())
            {
                var userAccount = context.Users.FirstOrDefault(user => user.Id == userId);

                if (userAccount != null && game.Players.Select(p => p.Guid).Contains(userAccount.PlayerGuid))
                {
                    ViewBag.IsPlayer = true;

                    if (game.GameState == Core.Game.State.Setup)
                    {
                        //todo - could restrict this to the game owner only?
                        ViewBag.CanUpdateGame = true;
                        ViewBag.CanStartGame = true;
                    }
                }
            }

            return View(new GameModel(game));
        }

        [HttpPost]
        public ActionResult Update(int gameId)
        {
            var game = GetGame(gameId);
            if (game.GameState != Core.Game.State.Setup)
            {
                throw new InvalidOperationException("Game already started");
            }

            for (var i = 0; i < game.AvailableCharacters.Count; i++)
            {
                game.AvailableCharacters[i] = (Character)Enum.Parse(typeof(Character), Request.Params["Character-" + i]);
            }

            game.Rules.Clear();
            foreach (var rule in Enum.GetValues(typeof(ResistanceOnline.Core.Rule)).Cast<ResistanceOnline.Core.Rule>())
            {
                if (!String.IsNullOrWhiteSpace(Request.Params[rule.ToString()]))
                {
                    game.Rules.Add(rule);
                }
            }

            for (var i = 0; i < game.RoundTables.Count; i++)
            {
                game.RoundTables[i].TeamSize = int.Parse(Request.Params["roundsize-" + i]);
                game.RoundTables[i].RequiredFails = int.Parse(Request.Params["roundfails-" + i]);
            }
            
            return RedirectToAction("Game", new { gameId = gameId });
        }

        [HttpPost]
        public ActionResult Join(int gameId)
        {
            var game = GetGame(gameId);
            if (game.GameState != Core.Game.State.Setup)
            {
                throw new InvalidOperationException("Game already started");
            }

            var userId = User.Identity.GetUserId();
            using (var context = new Database.ResistanceOnlineDbContext()) 
            {
                var userAccount = context.Users.FirstOrDefault(user => user.Id == userId);
                game.JoinGame(userAccount.UserName, userAccount.PlayerGuid);
            }

            return RedirectToAction("Game", new { gameId = gameId });
        }

        [HttpPost]
        public ActionResult Start(int gameId)
        {
            var game = GetGame(gameId);
            if (game.GameState != Core.Game.State.Setup)
            {
                throw new InvalidOperationException("Game already started");
            }

            game.StartGame();
            return Redirect("#/game/" + gameId.ToString());
        }


    }
}