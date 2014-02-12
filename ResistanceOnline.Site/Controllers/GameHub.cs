using Microsoft.AspNet.SignalR;
using ResistanceOnline.Core;
using ResistanceOnline.Site.Models;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace ResistanceOnline.Site.Controllers
{
    public class GameHub : Hub
    {
        //todo playerGuid
        static Dictionary<string, Guid> _players = new Dictionary<string, Guid>();

        Guid PlayerGuid
        {
            get
            {
                if (_players.ContainsKey(Context.ConnectionId))
                    return _players[Context.ConnectionId];

                return Guid.Empty;
            }
            set
            {
                _players[Context.ConnectionId] = value;
            }
        }

        static List<Game> _games = new List<Game>();

        private Game GetGame(int? gameId)
        {
            //todo - something to do with databases
            if (gameId.HasValue == false || gameId.Value >= _games.Count)
                return null;

            return _games[gameId.Value];
        }

       
        private void Update()
        {
            Clients.All.Update(_games.Select(g => new GameModel(g, PlayerGuid))); 
        }


        public override System.Threading.Tasks.Task OnConnected()
        {
            Update();
            return base.OnConnected();
        }        	
        
        public Game CreateGame(int players, bool impersonationEnabled)
        {
            var game = new Game(players,impersonationEnabled);
			_games.Add(game);
            game.GameId = _games.IndexOf(game);

            Update();

            return game;
        }       

        public void AddCharacter(int gameId, Guid playerGuid, string character) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.AddCharacter((Character)Enum.Parse(typeof(Character), character));

            Update();
        }

        public void AddToTeam(int gameId, Guid playerGuid, string person) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.AddToTeam(player, game.Players.First(p => p.Name == person));

            Update();
        }

        public void SubmitQuestCard(int gameId, Guid playerGuid, bool success) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.SubmitQuest(player, success);

            Update();
        }

        public void VoteForTeam(int gameId, Guid playerGuid, bool approve) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.VoteForTeam(player, approve);

            Update();
        }

        public void JoinGame(int gameId, string name) 
        {
            var game = GetGame(gameId);
            PlayerGuid = game.JoinGame(name);                       
            Update();
        }

        public void GuessMerlin(int gameId, Guid playerGuid, string guess) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.GuessMerlin(player, game.Players.First(p => p.Name == guess));
            Update();
        }
    }
}