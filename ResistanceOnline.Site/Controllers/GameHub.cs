using Microsoft.AspNet.SignalR;
using ResistanceOnline.Core;
<<<<<<< HEAD
using ResistanceOnline.Site.Models;
=======
>>>>>>> add signalr/durandal
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
<<<<<<< HEAD
using System.Web.Security;
=======
>>>>>>> add signalr/durandal

namespace ResistanceOnline.Site.Controllers
{
    public class GameHub : Hub
    {
<<<<<<< HEAD
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



=======
>>>>>>> add signalr/durandal
        static List<Game> _games = new List<Game>();

        public GameHub()
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
                var jordanGuid = game.JoinGame("Jordan");
<<<<<<< HEAD
                var jordan = game.Players.First(p => p.Guid == jordanGuid);
                var lukeGuid = game.JoinGame("Luke");
                var luke = game.Players.First(p => p.Guid == lukeGuid);
                var jeffreyGuid = game.JoinGame("Jeffrey");
                var jeffrey = game.Players.First(p => p.Guid == jeffreyGuid);
                var jayvinGuid = game.JoinGame("Jayvin");
                var jayvin = game.Players.First(p => p.Guid == jayvinGuid);
                var verneGuid = game.JoinGame("Verne");
                var verne = game.Players.First(p => p.Guid == verneGuid);
=======
                var jordan = game.Players.First(p=>p.Guid == jordanGuid);
                var lukeGuid = game.JoinGame("Luke");
                var luke = game.Players.First(p=>p.Guid == lukeGuid);
                var jeffreyGuid = game.JoinGame("Jeffrey");
                var jeffrey = game.Players.First(p=>p.Guid == jeffreyGuid);
                var jayvinGuid = game.JoinGame("Jayvin");
                var jayvin = game.Players.First(p=>p.Guid == jayvinGuid);
                var verneGuid = game.JoinGame("Verne");
                var verne = game.Players.First(p=>p.Guid == verneGuid);                
>>>>>>> add signalr/durandal

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

        private Game GetGame(int? gameId)
<<<<<<< HEAD
        {
=======
        {            
>>>>>>> add signalr/durandal
            //todo - something to do with databases
            if (gameId.HasValue == false || gameId.Value >= _games.Count)
                return null;

            return _games[gameId.Value];
        }

<<<<<<< HEAD
        private void Update()        
        {
            //todo playerGuid is the calling players Id not the player you're sending it to
            Clients.All.Update(_games.Select(g => new GameModel(g, PlayerGuid)));
        }


        public override System.Threading.Tasks.Task OnConnected()
        {
            //todo split into individual updates
            Update();
            return base.OnConnected();
        }

        public Game CreateGame(int players, bool impersonationEnabled)
        {
            //todo - something with the database :)
            var game = new Game(players, impersonationEnabled);
            _games.Add(game);
            game.GameId = _games.IndexOf(game);

            Update();

            return game;
        }

        public void AddCharacter(int gameId, string character)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.AddCharacter((Character)Enum.Parse(typeof(Character), character));

            Update();
        }

        public void AddToTeam(int gameId, string person)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.AddToTeam(player, game.Players.First(p => p.Name == person));

            Update();
        }

        public void SubmitQuestCard(int gameId, bool success)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.SubmitQuest(player, success);

            Update();
        }

        public void VoteForTeam(int gameId, bool approve)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.VoteForTeam(player, approve);

            Update();
        }

        public void JoinGame(int gameId, string name)
        {
            var game = GetGame(gameId);
            PlayerGuid = game.JoinGame(name);
            Update();
        }

        public void GuessMerlin(int gameId, string guess)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.GuessMerlin(player, game.Players.First(p => p.Name == guess));

            Update();
=======
        public override System.Threading.Tasks.Task OnConnected()
        {
            //todo split into individual updates
            Clients.Caller.Update(_games);
            return base.OnConnected();
        }        	
        
        public Game CreateGame(int players, bool impersonationEnabled)
        {
            //todo - something with the database :)
            var game = new Game(players,impersonationEnabled);
			_games.Add(game);
            game.GameId = _games.IndexOf(game);

            return game;
        }       

        public void AddCharacter(int gameId, Guid playerGuid, string character) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.AddCharacter((Character)Enum.Parse(typeof(Character), character));

            Clients.All.Update(_games);
        }

        public void AddToTeam(int gameId, Guid playerGuid, string person) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.AddToTeam(player, game.Players.First(p => p.Name == person));

            Clients.All.Update(_games);
        }

        public void SubmitQuestCard(int gameId, Guid playerGuid, bool success) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.SubmitQuest(player, success);

            Clients.All.Update(_games);
        }

        public void VoteForTeam(int gameId, Guid playerGuid, bool approve) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.VoteForTeam(player, approve);

            Clients.All.Update(_games);
        }

        public void JoinGame(int gameId, string name) 
        {
            var game = GetGame(gameId);
            var playerGuid = game.JoinGame(name);

            Clients.All.Update(_games);
        }

        public void GuessMerlin(int gameId, Guid playerGuid, string guess) 
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.GuessMerlin(player, game.Players.First(p => p.Name == guess));

            Clients.All.Update(_games);
>>>>>>> add signalr/durandal
        }
    }
}