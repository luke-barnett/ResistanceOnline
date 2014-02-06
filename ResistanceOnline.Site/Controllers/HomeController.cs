using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ResistanceOnline.Site.Controllers
{
	public class HomeController : Controller
	{
        Game GetGame(int gameId)
        {
            //todo - something with the database :)
            var game = new Game(6);

            game.AddCharacter(Character.Assassin);
            game.AddCharacter(Character.LoyalServantOfArthur);
            game.AddCharacter(Character.LoyalServantOfArthur);
            game.AddCharacter(Character.Percival);
            game.AddCharacter(Character.Morcana);
            game.AddCharacter(Character.Merlin);

            game.JoinGame("Jordan");
            game.JoinGame("Luke");
            game.JoinGame("Jeffrey");
            game.JoinGame("Simon");
            game.JoinGame("Verne");
            game.JoinGame("Jayvin");

            return game;
        }

		public ActionResult Index()
		{
			return View();
		}

        public ActionResult CreateGame()
        {
            throw new NotImplementedException();
        }

        //playerguid can be null for spectators
        public ActionResult Game(int gameId, Guid? playerGuid)
        {
            var game = GetGame(gameId);

            throw new NotImplementedException();
        }

        [HttpPost]
        public ActionResult AddCharacter(int gameId, Guid playerGuid, string character) 
        {
            throw new NotImplementedException();
        }        
        [HttpPost]
        public ActionResult ProposePersonForQuest(int gameId, Guid playerGuid, int playerIdOfProposed) 
        {
            throw new NotImplementedException();
        }        
        [HttpPost]
        public ActionResult SubmitQuestCard(int gameId, Guid playerGuid, bool success) 
        {
            throw new NotImplementedException();
        }        
        [HttpPost]
        public ActionResult VoteForQuest(int gameId, Guid playerGuid, bool accept) 
        {
            throw new NotImplementedException();
        }                         
        [HttpPost]
        public ActionResult AddCharacter(int gameId, Guid playerGuid, string character) 
        {
            throw new NotImplementedException();
        }        
        [HttpPost]
        public ActionResult JoinGame(int gameId, string name) 
        {
            throw new NotImplementedException();
        }        
        [HttpPost]
        public ActionResult GuessMerlin(int gameId, Guid playerGuid, int playerIdOfMerlin) 
        {
            throw new NotImplementedException();
        }        

	}
}