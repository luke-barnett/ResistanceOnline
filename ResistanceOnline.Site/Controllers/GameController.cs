using ResistanceOnline.Core;
using ResistanceOnline.Site.ComputerPlayers;
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
        static List<ComputerPlayer> _computerPlayers = new List<ComputerPlayer>();

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
                _computerPlayers.Add(new SimpleBot(game, game.JoinGame("Jordan")));
                _computerPlayers.Add(new SimpleBot(game, game.JoinGame("Luke")));
                _computerPlayers.Add(new SimpleBot(game, game.JoinGame("Jeffrey")));
                _computerPlayers.Add(new SimpleBot(game, game.JoinGame("Jayvin")));

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
            game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.AddCharacter, Character = (Character)Enum.Parse(typeof(Character), character) });
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }

        private void OnAfterAction(Game game)
        {
            var computersPlayersInGame = _computerPlayers.Where(c => game.Players.Select(p => p.Guid).Contains(c.PlayerGuid));
            while (computersPlayersInGame.Any(c => game.AvailableActions(game.Players.First(p => p.Guid == c.PlayerGuid)).Any()))
            {
                foreach (var computerPlayer in computersPlayersInGame)
                {
                    computerPlayer.DoSomething();
                }
            }
        }

        [HttpPost]
        public ActionResult AddToTeam(int gameId, Guid playerGuid, string person) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.AddToTeam, Player = game.Players.First(p => p.Name == person)});
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }        
        [HttpPost]
        public ActionResult SubmitQuestCard(int gameId, Guid playerGuid, bool success) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.SubmitQuestCard, Success = success });
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }        
        [HttpPost]
        public ActionResult VoteForTeam(int gameId, Guid playerGuid, bool approve) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.VoteForTeam, Accept = approve });
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }                             
        [HttpPost]
        public ActionResult JoinGame(int gameId, string name) 
        {
            var game = GetGame(gameId);
            var playerGuid = game.JoinGame(name);
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }

        [HttpPost]
        public ActionResult AddComputerPlayer(int gameId, Guid playerGuid, string bot, string name)
        {
            var game = GetGame(gameId);
            switch (bot)
            {
                case "simplebot":
                default:
                    _computerPlayers.Add(new ComputerPlayers.SimpleBot(game, game.JoinGame(name)));
                    break;
            }
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }        

        [HttpPost]
        public ActionResult GuessMerlin(int gameId, Guid playerGuid, string guess) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.GuessMerlin, Player = game.Players.First(p => p.Name == guess) });
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }
        [HttpPost]
        public ActionResult LadyOfTheLake(int gameId, Guid playerGuid, string target)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.UseTheLadyOfTheLake, Player = game.Players.First(p => p.Name == target) });
            OnAfterAction(game);
            return RedirectToAction("Game", new { gameId = gameId, playerGuid = playerGuid });
        }
	}
}