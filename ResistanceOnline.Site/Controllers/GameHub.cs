using Microsoft.AspNet.SignalR;
using ResistanceOnline.Core;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Site.Controllers
{
    public class GameHub : Hub
    {
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
        {            
            //todo - something to do with databases
            if (gameId.HasValue == false || gameId.Value >= _games.Count)
                return null;

            return _games[gameId.Value];
        }

        private void Update()
        {            
            Clients.All.Update(_games.Select(g => new GameModel(g, Guid.NewGuid()))); //playerGuid
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
            var playerGuid = game.JoinGame(name);

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