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
        static List<Game> _games = new List<Game>();

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
        public ActionResult CreateGame()
        {
            //todo - something with the database :)
            var game = new Game(6);

			_games.Add(game);

            return RedirectToAction("Game", new { gameId = _games.Count  });

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
            var guid = game.JoinGame("Verne");

            return RedirectToAction("Game", new { gameId = _games.Count, playerGuid = guid });
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
            ViewBag.GameState = game.DetermineState().ToString();

            //characters
            ViewBag.Characters = game.AvailableCharacters.Select(c=>c.ToString()).ToList();
            ViewBag.CharactersSelectList = new SelectList(Enum.GetNames(typeof(Character)).Where(c => c != Character.UnAllocated.ToString()).ToList());

            //player info
            var playerInfo = new List<string>();

            //me
            var player = game.Players.FirstOrDefault(p => p.Guid == playerGuid);
            if (player!=null) {
                playerInfo.Add("I am " + player.Character);
            }

            //other players
            foreach (var otherPlayer in game.Players.Where(p=>p!=player))
            {
                if (GameEngine.DetectEvil(player, otherPlayer))
                    playerInfo.Add(otherPlayer.Name + " is evil");
                if (GameEngine.DetectMerlin(player, otherPlayer))
                    playerInfo.Add(otherPlayer.Name + " could be merlin");
            }
            ViewBag.PlayerInfo = playerInfo;

            //game history
            var log = new List<string>();
            foreach (var round in game.Rounds)
            {
                log.Add("Round " + game.Rounds.IndexOf(round) + " - " + round.DetermineState().ToString());
                log.Add("Round size " + round.Size);
                log.Add("Required fails " + round.RequiredFails);
                foreach (var quest in round.Quests)
                {
                    log.Add("Quest " + round.Quests.IndexOf(quest));
                    log.Add("Quest Leader: " + quest.Leader.Name);
                    foreach (var p in quest.ProposedPlayers)
                    {
                        log.Add("Proposed player: " + p.Name);
                    }
                    foreach (var v in quest.Votes)
                    {
                        log.Add(v.Player.Name + " votes " + (quest.Votes.Count == round.TotalPlayers ? (v.Approve ? "Approve" : "Reject") : "submitted"));
                    }
                    if (quest.QuestCards.Count == round.Size)
                    {
                        foreach (var q in quest.QuestCards.Select(q=>q.Success).OrderBy(q=>q))
                        {
                            log.Add(q?"Success":"Fail");
                        }
                    }
                    else
                    {
                        foreach (var q in quest.QuestCards)
                        {
                            log.Add(q.Player.Name + " has submitted quest card");
                        }

                    }
                }
            }
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