using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ResistanceOnline.Site.Controllers
{
	public class HomeController : Controller
	{
        static Game _game = null;

        Game GetGame(int? gameId)
        {            
            //todo - something to do with databases
            if (gameId.HasValue == false)
                return null;

            return _game;
        }

		public ActionResult Index()
		{
            ViewBag.Games = new List<Game> { };
            if (_game != null) ViewBag.Games.Add(_game);
			return View();
		}

        [HttpPost]
        public ActionResult CreateGame()
        {
            //todo - something with the database :)
            _game = new Game(6);

            _game.AddCharacter(Character.LoyalServantOfArthur);
            _game.AddCharacter(Character.Assassin);
            _game.AddCharacter(Character.LoyalServantOfArthur);
            _game.AddCharacter(Character.Percival);
            _game.AddCharacter(Character.Morcana);
            _game.AddCharacter(Character.Merlin);
            
            _game.JoinGame("Jordan");
            _game.JoinGame("Luke");
            _game.JoinGame("Jeffrey");
            _game.JoinGame("Simon");
            _game.JoinGame("Jayvin");
            var guid = _game.JoinGame("Verne");

            return RedirectToAction("Game", new { gameId=1, playerGuid=guid });
        }

        //playerguid can be null for spectators
        public ActionResult Game(int? gameId, Guid? playerGuid)        
        {
            var game = GetGame(gameId);
            if (game == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.GameId = gameId;
            ViewBag.PlayerGuid = playerGuid;

            //impersonate
            ViewBag.Players = game.Players;

            //gamestate
            ViewBag.GameState = _game.DetermineState().ToString();

            //characters
            ViewBag.Characters = _game.AvailableCharacters.Select(c=>c.ToString()).ToList();
            ViewBag.CharactersSelectList = new SelectList(Enum.GetNames(typeof(Character)).ToList());

            //player info
            var playerInfo = new List<string>();

            //me
            var player = _game.Players.FirstOrDefault(p => p.Guid == playerGuid);
            if (player!=null) {
                playerInfo.Add("I am " + player.Character);
            }

            //other players
            foreach (var otherPlayer in _game.Players.Where(p=>p!=player))
            {
                if (GameEngine.DetectEvil(player, otherPlayer))
                    playerInfo.Add(otherPlayer.Name + " is evil");
                if (GameEngine.DetectMerlin(player, otherPlayer))
                    playerInfo.Add(otherPlayer.Name + " could be merlin");
            }
            ViewBag.PlayerInfo = playerInfo;

            //game history
            var log = new List<string>();
            ViewBag.Log = log;
            
            //actions
            ViewBag.PlayersSelectList = new SelectList(game.Players.Select(p => p.Name));
            ViewBag.Actions = GameEngine.AvailableActions(game, player);

            return View();
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
        public ActionResult ProposePersonForQuest(int gameId, Guid playerGuid, string person) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.ProposePlayer(game.Players.First(p => p.Name == person));
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