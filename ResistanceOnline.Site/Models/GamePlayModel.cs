using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Humanizer;
using System.Text;
using ResistanceOnline.Site.Infrastructure;

namespace ResistanceOnline.Site.Models
{
	public class GamePlayModel
	{
		public int GameId { get; set; }

		public Guid? PlayerGuid { get; set; }

		public string GameState { get; set; }

        public bool GameOver
        {
            get
            {

                return GameState == GamePlay.State.EternalChaos.ToString() || GameState == GamePlay.State.EvilTriumphs.ToString() || GameState == GamePlay.State.GoodPrevails.ToString();
            }
        }
		
		public List<Core.Player> ImpersonationList { get; set; }

		public List<string> CharactersInGame { get; set; }

		public List<SelectListItem> AllCharactersSelectList { get; set; }

        public SelectList LadyOfTheLakePlayerSelectList { get; set; }
        public SelectList GuessMerlinPlayersSelectList { get; set; }
        public SelectList AddToTeamPlayersSelectList { get; set; }
        public SelectList UseExcaliburSelectList { get; set; }
        public SelectList AssignExcaliburSelectList { get; set; }

		public List<string> Actions { get; set; }

		public string WaitingMessage { get; set; }

		public List<PlayerInfoModel> PlayerInfo { get; set; }

		public List<RoundModel> Rounds { get; set; }


        public List<string> RoundTables { get; set; }

        public List<string> LoyaltyCardsDeltInAdvance { get; set; }

		public bool IsSpectator { get; set; }

        public int GameSize { get; set; }

        public string GameSetup
        {
            get
            {
                StringBuilder setup = new StringBuilder();
                setup.AppendFormat("This is a {0} player game",GameSize.ToWords());
                if (CharactersInGame.Count > 0) 
                {
                    setup.AppendFormat(" with the following characters");
                }
                if (CharactersInGame.Count<GameSize) {
                    setup.AppendFormat(" (waiting on {0} to be added)", "more character".ToQuantity(GameSize - CharactersInGame.Count, ShowQuantityAs.Words));
                }
                return setup.ToString();
            }
        }

		public List<SelectListItem> AllRulesSelectList { get; set; }

		public List<string> Rules { get; set; }

		public GamePlayModel(GamePlay gameplay, Guid? playerGuid)
		{
            GameId = gameplay.Game.GameId;
			PlayerGuid = playerGuid;
            GameSize = gameplay.Game.Players.Count;
            AssassinIsInTheGame = gameplay.Game.Players.Select(p => p.Character).Contains(Character.Assassin);
            Rules = gameplay.Game.Rules.Select(r => r.Humanize()).ToList();

            RoundTables = gameplay.Game.RoundTables.Select(t=>String.Format("Round {0} has {1} and requires {2}", (gameplay.Game.RoundTables.IndexOf(t) + 1).ToWords(), "player".ToQuantity(t.TeamSize, ShowQuantityAs.Words), "fail".ToQuantity(t.RequiredFails, ShowQuantityAs.Words))).ToList();

            LoyaltyCardsDeltInAdvance = new List<string>();
            if (gameplay.Game.Rules.Contains(Rule.LoyaltyCardsDeltInAdvance) && gameplay.Game.ContainsLancelot())
            {
                for (int i = 0; i < 5; i++)
                {
                    LoyaltyCardsDeltInAdvance.Add(string.Format("Round {0} - {1}", (i + 1).ToWords(), gameplay.Game.LoyaltyDeck[i].Humanize()));
                }
            }

            var player = gameplay.Game.Players.FirstOrDefault(p => p.Guid == playerGuid);
			IsSpectator = player == null;

            PlayerName = player == null ? "Spectator" : player.Name;

            AssassinsGuessAtMerlin = gameplay.AssassinsGuessAtMerlin;
			GameState = gameplay.GamePlayState.ToString();
            CharactersInGame = gameplay.Game.AvailableCharacters.Select(i => i.ToString()).ToList();
		
            AllCharactersSelectList =
				Enum.GetValues(typeof(Character))
					.Cast<Character>()
					.Select(c => new SelectListItem { Text = c.Humanize(LetterCasing.Sentence), Value = c.ToString() })
					.ToList();

			AllRulesSelectList =
				Enum.GetValues(typeof(Rule))
					.Cast<Rule>()
					.Select(r => new SelectListItem { Text = r.Humanize(LetterCasing.Sentence), Value = r.ToString() })
					.ToList();

            //can guess anyone but self
            GuessMerlinPlayersSelectList = new SelectList(gameplay.Game.Players.Where(p => p != player).Select(p => p.Name));

            //can use on anyone who hasn't had it
            var ladyOfTheLakeHistory = gameplay.Rounds.Where(r=>r.LadyOfTheLake!=null).Select(r => r.LadyOfTheLake.Holder);
            LadyOfTheLakePlayerSelectList = new SelectList(gameplay.Game.Players.Where(p => p != player).Except(ladyOfTheLakeHistory).Select(p => p.Name));

            if (gameplay.CurrentRound != null && gameplay.CurrentRound.CurrentTeam != null)
            {
                UseExcaliburSelectList = new SelectList(gameplay.CurrentRound.CurrentTeam.TeamMembers.Where(p => p != gameplay.CurrentRound.CurrentTeam.Leader).Select(p => p.Name));
                AssignExcaliburSelectList = new SelectList(gameplay.CurrentRound.CurrentTeam.TeamMembers.Select(p => p.Name));
            }

            //can put anyone on a team who isn't already on it
            if (gameplay.CurrentRound != null)
            {
                AddToTeamPlayersSelectList = new SelectList(gameplay.Game.Players.Where(p => !gameplay.CurrentRound.CurrentTeam.TeamMembers.Select(t => t.Name).ToList().Contains(p.Name)).Select(p => p.Name));
            }

			Actions = gameplay.AvailableActions(player).Select(i => i.ToString()).ToList();         

			PlayerInfo = new List<PlayerInfoModel>();
			var waiting = new List<WaitingActionsModel>();
            foreach (var p in gameplay.Game.Players)
			{
				var playerInfo = new PlayerInfoModel 
				{ 
					Name = p.Name, 
					Knowledge = gameplay.PlayerKnowledge(player, p).ToString()
				};

				//always know own character, or all characters if game is over
                if (p == player || GameOver)
                {
                    playerInfo.CharacterCard = p.Character;
                    playerInfo.Knowledge = p.Character.ToString();
                }

				PlayerInfo.Add(playerInfo);

				waiting.AddRange(gameplay.AvailableActions(p).Select(a => new WaitingActionsModel { Action = a, Name = p.Name }));
			}

            //build waiting message
            List<string> waitings = new List<string>();
            foreach (var action in waiting.Select(w => w.Action).Distinct())                
            {
                var players = "someone";
                if (waiting.Count(w => w.Action == action) < GameSize)
                {
                    players = Useful.CommaQuibbling(waiting.Where(w => w.Action == action).Select(w => w.Name).ToList());
                }
                waitings.Add(String.Format("{0} to {1}", players, action.Humanize(LetterCasing.LowerCase)));
            }
            if (waitings.Any())
            {
                WaitingMessage = String.Format("Waiting for {0}.", string.Join(" or ", waitings));
            }
			
			Rounds = new List<RoundModel>();
			for(int i=0; i<gameplay.Rounds.Count; i++)
			{
                Rounds.Add(new RoundModel(gameplay.Rounds[i], i + 1, gameplay, player));
			}          
		}

        public string PlayerName { get; set; }

        public Player AssassinsGuessAtMerlin { get; set; }
        public bool AssassinIsInTheGame { get; set; }
    }
}