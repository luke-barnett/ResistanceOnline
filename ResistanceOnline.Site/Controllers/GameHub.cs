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
using Action = ResistanceOnline.Core.Action;


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
            if (_gameSetups.Count == 0)
            {
                var setup = new GameSetup();
                var game = new Game(setup);
                setup.Rules.Clear();
                setup.Rules.Add(Rule.LancelotsKnowEachOther);
                setup.Rules.Add(Rule.GoodMustAlwaysVoteSucess);
                setup.Rules.Add(Rule.IncludeLadyOfTheLake);
                
				_computerPlayers.Add(new TrustBot(game, setup.JoinGame("\"Jordan\"", Guid.NewGuid())));
                _computerPlayers.Add(new TrustBot(game, setup.JoinGame("\"Luke\"", Guid.NewGuid())));
                _computerPlayers.Add(new TrustBot(game, setup.JoinGame("\"Jeffrey\"", Guid.NewGuid())));
                _computerPlayers.Add(new TrustBot(game, setup.JoinGame("\"Jayvin\"", Guid.NewGuid())));

                setup.SetCharacter(0, Character.Merlin);
                setup.SetCharacter(1, Character.Assassin);
                setup.SetCharacter(2, Character.Percival);
                setup.SetCharacter(3, Character.Morgana);

                _gameSetups.Add(setup);
                setup.GameId = _gameSetups.IndexOf(setup);
            }
        }

        //todo logged in user
        private Guid PlayerGuid
        {
            get
            {
                return CurrentUser.PlayerGuid;
            }
        }

        private UserAccount CurrentUser
        {
            get
            {
                UserAccount userAccount = null;
                if (Context.User != null)
                {
                    var userId = Context.User.Identity.GetUserId();
                    userAccount = _dbContext.Users.FirstOrDefault(user => user.Id == userId);
                }
                if (userAccount == null)
                {
                    userAccount = new UserAccount { PlayerGuid = Guid.Empty };
                }
                return userAccount;
            }
        }


        static List<GameSetup> _gameSetups = new List<GameSetup>();
        static List<Action> _actions = new List<Action>();
        static List<ComputerPlayer> _computerPlayers = new List<ComputerPlayer>();
        static Dictionary<Guid, List<string>> _userConnections = new Dictionary<Guid, List<string>>();


        private Game GetGame(int? gameId)
        {
            //todo - something to do with databases
            if (gameId.HasValue == false || gameId.Value >= _gameSetups.Count)
                return null;

            var game = new Game(_gameSetups[gameId.Value]);
            game.DoActions(_actions.Where(a => a.GameId == gameId).ToList());
            return game;
        }

        private void Update()
        {
            foreach (var guid in _userConnections.Keys)
            {
                var games = _gameSetups.Select(g => new GameModel(GetGame(g.GameId), guid));

                foreach (var connection in _userConnections[guid])
                {
                    Clients.Client(connection).Update(games);
                }
            }
        }

        private void OnAfterAction(Game game)
        {
            var state = game.GameState;
            if (state != Core.Game.State.Setup)
            {
                var computersPlayersInGame = _computerPlayers.Where(c => game.Setup.Players.Select(p => p.Guid).Contains(c.PlayerGuid));
                while (computersPlayersInGame.Any(c => game.AvailableActions(game.Setup.Players.First(p => p.Guid == c.PlayerGuid)).Any()))
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
            var gameSetup = new GameSetup();
            _gameSetups.Add(gameSetup);
            gameSetup.GameId = _gameSetups.IndexOf(gameSetup);

            Update();

            return new Game(gameSetup);
        }

        public void SetCharacter(int gameId, int index, string character)
        {
            var game = GetGame(gameId);
            game.Setup.SetCharacter(index, (Character)Enum.Parse(typeof(Character), character));
            OnAfterAction(game);

            Update();
        }

		public void AddRule(int gameId, string rule)
		{
			var game = GetGame(gameId);
            game.Setup.AddRule((Core.Rule)Enum.Parse(typeof(Core.Rule), rule));
			OnAfterAction(game);

			Update();
		}

        public void AddToTeam(int gameId, string person)
        {
            var game = GetGame(gameId);
            var player = game.Setup.Players.First(p => p.Guid == PlayerGuid);
            game.DoAction(gameId, player, Action.Type.AddToTeam, game.Setup.Players.First(p => p.Name == person));
            OnAfterAction(game);

            Update();
        }

        public void SubmitQuestCard(int gameId, bool success)
        {
            var game = GetGame(gameId);
            var player = game.Setup.Players.First(p => p.Guid == PlayerGuid);
            game.DoAction(gameId, player, success ? Action.Type.SucceedQuest : Action.Type.FailQuest);
            OnAfterAction(game);

            Update();
        }

        public void VoteForTeam(int gameId, bool approve)
        {
            var game = GetGame(gameId);
            var player = game.Setup.Players.First(p => p.Guid == PlayerGuid);
            game.DoAction(gameId, player, approve ? Action.Type.VoteApprove : Action.Type.VoteReject);
            OnAfterAction(game);

            Update();
        }

        public void JoinGame(int gameId)
        {
            var game = GetGame(gameId);
            game.Setup.JoinGame(CurrentUser.UserName, PlayerGuid);
            OnAfterAction(game);

            Update();
        }

        public void GuessMerlin(int gameId, string guess)
        {
            var game = GetGame(gameId);
            var player = game.Setup.Players.First(p => p.Guid == PlayerGuid);
            game.DoAction(gameId, player, Action.Type.GuessMerlin, game.Setup.Players.First(p => p.Name == guess));
            OnAfterAction(game);

            Update();
        }

        public void LadyOfTheLake(int gameId, string target)
        {
            var game = GetGame(gameId);
            var player = game.Setup.Players.First(p => p.Guid == PlayerGuid);
            game.DoAction(gameId, player, Action.Type.UseTheLadyOfTheLake, game.Setup.Players.First(p => p.Name == target));
            OnAfterAction(game);

            Update();
        }


        public void Message(int gameId, string message)
        {
            var game = GetGame(gameId);
            var player = game.Setup.Players.First(p => p.Guid == PlayerGuid);
            game.Message(player, message);
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

        public void AssignExcalibur(int gameId, string proposedPlayerName)
        {
            var game = GetGame(gameId);
            var player = game.Setup.Players.First(p => p.Guid == PlayerGuid);
            var proposedPlayer = game.Setup.Players.First(p => p.Name == proposedPlayerName);
            game.DoAction(gameId, player, Action.Type.AssignExcalibur, game.Setup.Players.First(p => p.Name == proposedPlayerName));
            OnAfterAction(game);
            Update();
        }

        public void UseExcalibur(int gameId, string proposedPlayerName)
        {
            var game = GetGame(gameId);
            var player = game.Setup.Players.First(p => p.Guid == PlayerGuid);
            var proposedPlayer = game.Setup.Players.First(p => p.Name == proposedPlayerName);
            game.DoAction(gameId, player, Action.Type.UseExcalibur, game.Setup.Players.First(p => p.Name == proposedPlayerName));
            OnAfterAction(game);
            Update();
        }

        public void AddComputerPlayer(int gameId, string bot, string name)
        {
            var game = GetGame(gameId);
            switch (bot)
            {
                case "trustbot":
                    _computerPlayers.Add(new ComputerPlayers.TrustBot(game, game.Setup.JoinGame(name, Guid.NewGuid())));
                    break;
                case "cheatbot":
                    _computerPlayers.Add(new ComputerPlayers.CheatBot(game, game.Setup.JoinGame(name, Guid.NewGuid())));
                    break;
                case "simplebot":
                default:
                    _computerPlayers.Add(new ComputerPlayers.SimpleBot(game, game.Setup.JoinGame(name, Guid.NewGuid())));
                    break;
            }
            OnAfterAction(game);
            Update();
        }
    }
}