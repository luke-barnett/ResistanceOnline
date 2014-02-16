using Microsoft.AspNet.SignalR;
using ResistanceOnline.Core;
using ResistanceOnline.Site.Models;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Site.Controllers
{
    public class GameHub : Hub
    {
        //todo logged in user
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
            //todo playerGuid is the calling players Id not the player you're sending it to
            Clients.All.Update(_games.Select(g => new GameModel(g, PlayerGuid)));
        }


        public override System.Threading.Tasks.Task OnConnected()
        {
            Update();
            return base.OnConnected();
        }

        public Game CreateGame(int players)
        {
            var game = new Game(players);
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
            PlayerGuid = Guid.NewGuid();
            game.JoinGame(name, PlayerGuid);
            Update();
        }

        public void GuessMerlin(int gameId, string guess)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.GuessMerlin(player, game.Players.First(p => p.Name == guess));
            Update();
        }

        public void LadyOfTheLake(int gameId, Guid playerGuid, string target)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == playerGuid);
            game.UseLadyOfTheLake(player, game.Players.First(p => p.Name == target));
            Update();
        }
    }
}