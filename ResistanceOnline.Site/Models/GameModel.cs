using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Humanizer;

namespace ResistanceOnline.Site.Models
{
	public class GameModel
	{
		public int GameId { get; set; }

		public Guid? PlayerGuid { get; set; }

		public Core.Game.State GameState { get; set; }

        public bool GameOver
        {
            get
            {
                return (GameState == Game.State.GoodPrevails || GameState == Game.State.MerlinDies || GameState == Game.State.EvilTriumphs);
            }
        }
		
		public List<Core.Player> ImpersonationList { get; set; }

		public List<Core.Character> CharactersInGame { get; set; }

		public List<SelectListItem> AllCharactersSelectList { get; set; }

        public SelectList LadyOfTheLakePlayerSelectList { get; set; }
        public SelectList GuessMerlinPlayersSelectList { get; set; }
        public SelectList AddToTeamPlayersSelectList { get; set; }

		public List<Core.Action.Type> Actions { get; set; }

		public List<WaitingActionsModel> Waiting { get; set; }

		public List<PlayerInfoModel> PlayerInfo { get; set; }

		public List<RoundModel> Rounds { get; set; }

        public List<RoundTable> RoundTables { get; set; } 

		public bool IsSpectator { get; set; }

        public int GameSize { get; set; }

        public int CharactersMissing { get { return GameSize - CharactersInGame.Count; } }

        public int PlayersMissing { get; private set; }

		public GameModel(Game game, Guid? playerGuid)
		{
			GameId = game.GameId;
			PlayerGuid = playerGuid;
            GameSize = game.GameSize;
            PlayersMissing = game.GameSize - game.Players.Count;

            RoundTables = game.RoundTables;

			var player = game.Players.FirstOrDefault(p => p.Guid == playerGuid);
			IsSpectator = player == null;

            PlayerName = player == null ? "Spectator" : player.Name;

            AssassinsGuessAtMerlin = game.AssassinsGuessAtMerlin;
			GameState = game.DetermineState();
			CharactersInGame = game.AvailableCharacters.ToList();
			AllCharactersSelectList =
				Enum.GetValues(typeof(Character))
					.Cast<Character>()
					.Where(c => c != Character.UnAllocated)
					.Select(c => new SelectListItem { Text = c.Humanize(LetterCasing.Sentence), Value = c.ToString() })
					.ToList();

            //can guess anyone but self
            GuessMerlinPlayersSelectList = new SelectList(game.Players.Where(p=>p!=player).Select(p => p.Name));

            //can use on anyone who hasn't had it
            LadyOfTheLakePlayerSelectList = new SelectList(game.Players.Where(p => p != player).Except(game.LadyOfTheLakeUses.Select(u => u.UsedBy)).Select(p => p.Name));

            //can put anyone on a team who isn't already on it
			AddToTeamPlayersSelectList = new SelectList(game.Players.Where(p=> !game.CurrentRound.CurrentTeam.TeamMembers.Select(t=>t.Name).ToList().Contains(p.Name)).Select(p => p.Name));

			Actions = game.AvailableActions(player);

			PlayerInfo = new List<PlayerInfoModel>();
			Waiting = new List<WaitingActionsModel>();
			foreach (var p in game.Players)
			{
				var playerInfo = new PlayerInfoModel 
				{ 
					Name = p.Name, 
					Knowledge = game.PlayerKnowledge(player, p)
				};

				//always know own character, or all characters if game is over
				if ((p==player || GameState == Game.State.EvilTriumphs || GameState == Game.State.GoodPrevails || GameState == Game.State.MerlinDies) && p.Character != Character.UnAllocated) {
					playerInfo.CharacterCard = p.Character; 
				}

				PlayerInfo.Add(playerInfo);

				Waiting.AddRange(game.AvailableActions(p).Select(a => new WaitingActionsModel { Action = a, Name = p.Name }));
			}
			
			//game history
			Rounds = new List<RoundModel>();
			foreach (var round in game.Rounds)
			{
				Rounds.Add(new RoundModel(round));
			}
		}

        public object PlayerName { get; set; }

        public string CommaQuibbling(IEnumerable<string> items)
        {
            var itemArray = items.ToArray();

            var commaSeparated = String.Join(", ", itemArray, 0, Math.Max(itemArray.Length - 1, 0));
            if (commaSeparated.Length > 0) commaSeparated += " and ";

            return commaSeparated + itemArray.LastOrDefault();
        }

        public Player AssassinsGuessAtMerlin { get; set; }

    }
}