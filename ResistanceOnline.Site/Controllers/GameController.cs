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

namespace ResistanceOnline.Site.Controllers
{
	[Authorize]
	public class GameController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

        private GameSetup GetGameSetup(int gameId)
        {
            return GameHub.GameSetups.Single(s => s.GameId == gameId);
        }

        public ActionResult Setup(int gameId)
        {
            var setup = GetGameSetup(gameId);

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

            return View(setup);
        }

        public ActionResult UpdateSetup(int gameId)
        {
            var setup = GetGameSetup(gameId);

            for (var i = 0; i < setup.AvailableCharacters.Count; i++)
            {
                setup.AvailableCharacters[i] = (Character)Enum.Parse(typeof(Character), Request.Params["Character-" + i]);
            }

            setup.Rules.Clear();
            foreach (var rule in Enum.GetValues(typeof(ResistanceOnline.Core.Rule)).Cast<ResistanceOnline.Core.Rule>())
            {
                if (!String.IsNullOrWhiteSpace(Request.Params[rule.ToString()]))
                {
                    setup.Rules.Add(rule);
                }
            }

            for (var i = 0; i < setup.RoundTables.Count; i++)
            {
                setup.RoundTables[i].TeamSize = int.Parse(Request.Params["roundsize-" + i]);
                setup.RoundTables[i].RequiredFails = int.Parse(Request.Params["roundfails-" + i]);
            }

            return View("Setup", setup);
        }
	}
}