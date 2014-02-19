using Microsoft.AspNet.SignalR;
using ResistanceOnline.Core;
using ResistanceOnline.Database;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Web.Mvc;
using ResistanceOnline.Database.Entities;
using ResistanceOnline.Site.ComputerPlayers;


namespace ResistanceOnline.Site.Controllers
{
    //[Authorize]
    public class GameHub : Hub
    {
        readonly ResistanceOnlineDbContext _dbContext;
        public GameHub()//(ResistanceOnlineDbContext dbContext)
        {
            _dbContext = new Database.ResistanceOnlineDbContext(); //todo injection

            //create a default game to make development easier
            if (_games.Count == 0)
            {
                var game = new Game();
                game.Rule_LancelotsKnowEachOther = true;
                game.Rule_GoodMustAlwaysVoteSucess = true;
                game.Rule_IncludeLadyOfTheLake = true;
                //game.Rule_LoyaltyCardsDeltInAdvance = true;
                game.AddCharacter(Character.LoyalServantOfArthur);
                game.AddCharacter(Character.Assassin);
                game.AddCharacter(Character.Percival);
                game.AddCharacter(Character.Morgana);
                game.AddCharacter(Character.Merlin);
                _computerPlayers.Add(new TrustBot(game, game.JoinGame("Jordan", Guid.NewGuid())));
                _computerPlayers.Add(new TrustBot(game, game.JoinGame("Luke", Guid.NewGuid())));
                _computerPlayers.Add(new TrustBot(game, game.JoinGame("Jeffrey", Guid.NewGuid())));
                _computerPlayers.Add(new TrustBot(game, game.JoinGame("Jayvin", Guid.NewGuid())));
                
                _games.Add(game);
                game.GameId = _games.IndexOf(game);
            }
        }

        //todo logged in user
        private Guid PlayerGuid
        {
            get
            {
                if (CurrentUser != null)
                {
                    return CurrentUser.PlayerGuid;
                }
                return Guid.Empty;
            }
        }

        private UserAccount CurrentUser
        {
            get
            {
                if (Context.User == null)
                    return null;
                var userId = Context.User.Identity.GetUserId();
                return _dbContext.Users.FirstOrDefault(user => user.Id == userId);
            }
        }


        static List<Game> _games = new List<Game>();
        static List<ComputerPlayer> _computerPlayers = new List<ComputerPlayer>();
        static Dictionary<Guid, List<string>> _userConnections = new Dictionary<Guid, List<string>>();


        private Game GetGame(int? gameId)
        {
            //todo - something to do with databases
            if (gameId.HasValue == false || gameId.Value >= _games.Count)
                return null;

            return _games[gameId.Value];
        }

        private void Update()
        {
            foreach (var guid in _userConnections.Keys)
            {
                var games = _games.Select(g => new GameModel(g, guid));

                foreach (var connection in _userConnections[guid])
                {
                    Clients.Client(connection).Update(games);
                }
            }
        }

        private void OnAfterAction(Game game)
        {
            var state = game.DetermineState();
            if (state == Core.Game.State.Playing || state == Core.Game.State.GuessingMerlin)
            {
                var computersPlayersInGame = _computerPlayers.Where(c => game.Players.Select(p => p.Guid).Contains(c.PlayerGuid));
                while (computersPlayersInGame.Any(c => game.AvailableActions(game.Players.First(p => p.Guid == c.PlayerGuid)).Any(a => a != Core.Action.Type.Message)))
                {
                    foreach (var computerPlayer in computersPlayersInGame)
                    {
                        computerPlayer.DoSomething();
                    }
                }
            }
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            //todo cleanup on disconnection
            if (PlayerGuid != Guid.Empty)
            {
                if (!_userConnections.ContainsKey(PlayerGuid))
                {
                    _userConnections[PlayerGuid] = new List<string>();
                }
                _userConnections[PlayerGuid].Add(Context.ConnectionId);
            }
            Update();
            return base.OnConnected();
        }

        public Game CreateGame(int players)
        {
            //todo - something with the database :)
            var game = new Game();
            _games.Add(game);
            game.GameId = _games.IndexOf(game);

            Update();

            return game;
        }

        public void AddCharacter(int gameId, string character)
        {
            var game = GetGame(gameId);
            game.AddCharacter((Character)Enum.Parse(typeof(Character), character));
            OnAfterAction(game);

            Update();
        }

        public void AddToTeam(int gameId, string person)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.AddToTeam(player, game.Players.First(p => p.Name == person));
            OnAfterAction(game);

            Update();
        }

        public void SubmitQuestCard(int gameId, bool success)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.SubmitQuest(player, success);
            OnAfterAction(game);

            Update();
        }

        public void VoteForTeam(int gameId, bool approve)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.VoteForTeam(player, approve);
            OnAfterAction(game);

            Update();
        }

        public void JoinGame(int gameId)
        {
            var game = GetGame(gameId);
            game.JoinGame(CurrentUser.UserName, PlayerGuid);
            OnAfterAction(game);

            Update();
        }

        public void GuessMerlin(int gameId, string guess)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.GuessMerlin(player, game.Players.First(p => p.Name == guess));
            OnAfterAction(game);

            Update();
        }

        public void LadyOfTheLake(int gameId, string target)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.UseLadyOfTheLake(player, game.Players.First(p => p.Name == target));
            OnAfterAction(game);

            Update();
        }


        public void Message(int gameId, string message)
        {
            var game = GetGame(gameId);
            var player = game.Players.First(p => p.Guid == PlayerGuid);
            game.PerformAction(player, new Core.Action { ActionType = Core.Action.Type.Message, Message = message });
            OnAfterAction(game);

            Update();
        }

        public void StartGame(int gameId)
        {
            var game = GetGame(gameId);
            game.StartGame();
            OnAfterAction(game);
            Update();
        }


        public void AddComputerPlayer(int gameId, string bot, string name)
        {
            var game = GetGame(gameId);
            switch (bot)
            {
                case "trustbot":
                    _computerPlayers.Add(new ComputerPlayers.TrustBot(game, game.JoinGame(name, Guid.NewGuid())));
                    break;
                case "cheatbot":
                    _computerPlayers.Add(new ComputerPlayers.CheatBot(game, game.JoinGame(name, Guid.NewGuid())));
                    break;
                case "simplebot":
                default:
                    _computerPlayers.Add(new ComputerPlayers.SimpleBot(game, game.JoinGame(name, Guid.NewGuid())));
                    break;
            }
            OnAfterAction(game);
            Update();
        }
    }
}