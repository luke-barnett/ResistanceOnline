using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Humanizer;

namespace ResistanceOnline.Site.Models
{
	public class GameViewModel
	{
		public int GameId { get; set; }

		public Guid? PlayerGuid { get; set; }

		public Core.Game.State GameState { get; set; }
		
		public List<Core.Player> ImpersonationList { get; set; }

		public List<Core.Character> CharactersInGame { get; set; }

		public List<SelectListItem> AllCharactersSelectList { get; set; }

		public SelectList PlayersSelectList { get; set; }

		public List<Core.Action.Type> Actions { get; set; }

		public List<WaitingActionsModel> Waiting { get; set; }

		public List<PlayerInfoModel> PlayerInfo { get; set; }

		public List<RoundModel> Rounds { get; set; }

		public bool IsSpectator { get; set; }

        public int GameSize { get; set; }

        public int CharactersMissing { get { return GameSize - CharactersInGame.Count; } }

		public GameViewModel(Game game, Guid? playerGuid)
		{
			GameId = game.GameId;
			PlayerGuid = playerGuid;
            GameSize = game.TotalPlayers;

			var player = game.Players.FirstOrDefault(p => p.Guid == playerGuid);
			IsSpectator = player == null;

			if (game.ImpersonationEnabled)
			{
				ImpersonationList = game.Players.ToList();
			}

			GameState = game.DetermineState();
			CharactersInGame = game.AvailableCharacters.ToList();
			AllCharactersSelectList =
				Enum.GetValues(typeof(Character))
					.Cast<Character>()
					.Where(c => c != Character.UnAllocated)
					.Select(c => new SelectListItem { Text = c.Humanize(LetterCasing.Sentence), Value = c.ToString() })
					.ToList();
			PlayersSelectList = new SelectList(game.Players.Select(p => p.Name));
			Actions = game.AvailableActions(player);

			PlayerInfo = new List<PlayerInfoModel>();
			Waiting = new List<WaitingActionsModel>();
			foreach (var p in game.Players)
			{
				var playerInfo = new PlayerInfoModel 
				{ 
					Name = p.Name, 
					CouldBeMerlin = Game.DetectMerlin(player, p), 
					IsEvil = Game.DetectEvil(player, p) 
				};

				//always know own character, or all characters if game is over
				if ((p==player || GameState == Game.State.EvilTriumphs || GameState == Game.State.GoodPrevails || GameState == Game.State.MerlinDies) && p.Character != Character.UnAllocated) {
					playerInfo.CharacterCard = p.Character; 
				}

				PlayerInfo.Add(playerInfo);

				Waiting.AddRange(game.AvailableActions(p).Select(a => new WaitingActionsModel { Action = a.Humanize(LetterCasing.Sentence), Name = p.Name }));
			}
			
			//game history
			Rounds = new List<RoundModel>();
			foreach (var round in game.Rounds)
			{
				Rounds.Add(new RoundModel(round));
			}
		}


		public List<string> Log { get; set; }
	}
}