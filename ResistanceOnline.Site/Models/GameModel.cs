using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Humanizer;
using System.Text;

namespace ResistanceOnline.Site.Models
{
	public class GameModel
	{
		public int GameId { get; set; }

		public Guid? PlayerGuid { get; set; }

		public string GameState { get; set; }

        public bool GameOver
        {
            get
            {

                return (GameState == Game.State.GoodPrevails.ToString() || GameState == Game.State.MerlinDies.ToString() || GameState == Game.State.EvilTriumphs.ToString());
            }
        }
		
		public List<Core.Player> ImpersonationList { get; set; }

		public List<string> CharactersInGame { get; set; }

		public List<SelectListItem> AllCharactersSelectList { get; set; }

        public SelectList LadyOfTheLakePlayerSelectList { get; set; }
        public SelectList GuessMerlinPlayersSelectList { get; set; }
        public SelectList AddToTeamPlayersSelectList { get; set; }

		public List<string> Actions { get; set; }

		public string WaitingMessage { get; set; }

		public List<PlayerInfoModel> PlayerInfo { get; set; }

		public List<RoundModel> Rounds { get; set; }


        public List<string> RoundTables { get; set; } 

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

        public int PlayersMissing { get; private set; }

		public GameModel(Game game, Guid? playerGuid)
		{
			GameId = game.GameId;
			PlayerGuid = playerGuid;
            GameSize = game.GameSize;
            PlayersMissing = game.GameSize - game.Players.Count;
            AssassinIsInTheGame = game.Players.Select(p => p.Character).Contains(Character.Assassin);

            RoundTables = game.RoundTables.Select(t=>String.Format("Round {0} has {1} and requires {2}", (game.RoundTables.IndexOf(t) + 1).ToWords(), "player".ToQuantity(t.TeamSize, ShowQuantityAs.Words), "fail".ToQuantity(t.RequiredFails, ShowQuantityAs.Words))).ToList();

			var player = game.Players.FirstOrDefault(p => p.Guid == playerGuid);
			IsSpectator = player == null;

            PlayerName = player == null ? "Spectator" : player.Name;

            AssassinsGuessAtMerlin = game.AssassinsGuessAtMerlin;
			GameState = game.DetermineState().ToString();
			CharactersInGame = game.AvailableCharacters.Select(i => i.ToString()).ToList();
		
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
            if (game.CurrentRound != null)
            {
                AddToTeamPlayersSelectList = new SelectList(game.Players.Where(p => !game.CurrentRound.CurrentTeam.TeamMembers.Select(t => t.Name).ToList().Contains(p.Name)).Select(p => p.Name));
            }

			Actions = game.AvailableActions(player).OrderBy(x=>x != Core.Action.Type.Message).Select(i => i.ToString()).ToList();

			PlayerInfo = new List<PlayerInfoModel>();
			var waiting = new List<WaitingActionsModel>();
			foreach (var p in game.Players)
			{
				var playerInfo = new PlayerInfoModel 
				{ 
					Name = p.Name, 
					Knowledge = game.PlayerKnowledge(player, p).ToString()
				};

				//always know own character, or all characters if game is over
                if ((p == player || GameState == Game.State.EvilTriumphs.ToString() || GameState == Game.State.GoodPrevails.ToString() || GameState == Game.State.MerlinDies.ToString()) && p.Character != Character.UnAllocated)
                {
                    playerInfo.CharacterCard = p.Character;
                    playerInfo.Knowledge = p.Character.ToString();
                }

				PlayerInfo.Add(playerInfo);

				waiting.AddRange(game.AvailableActions(p).Select(a => new WaitingActionsModel { Action = a, Name = p.Name }));
			}

            //build waiting message
            if (waiting.Count > 0)
            {
                List<string> waitings = new List<string>();
                foreach (var action in waiting.Where(w=> w.Action != Core.Action.Type.Message).Select(w => w.Action).Distinct())                
                {
                    var players = "someone";
                    if (waiting.Count(w => w.Action == action) < GameSize)
                    {
                        players = CommaQuibbling(waiting.Where(w => w.Action == action).Select(w => w.Name).ToList());
                    }
                    waitings.Add(String.Format("{0} to {1}", players, action.Humanize(LetterCasing.LowerCase)));
                }
                WaitingMessage = String.Format("Waiting for {0}.", string.Join(" or ", waitings));
            }
			
			//game history
			Rounds = new List<RoundModel>();
			for(int i=0; i<game.Rounds.Count; i++)
			{
                Rounds.Add(new RoundModel(game.Rounds[i], i + 1, game, player));
			}

            if (Rounds.Count > 0)
            {
                var currentRound = Rounds.Last();
                var currentTeam = currentRound.Teams.Last();
                if (currentTeam.TeamMembers.Count < currentRound.TeamSize)
                {
                    //currentTeam.WaitingMessage = String.Format("Waiting for {0} to choose {1}", currentTeam.Leader, "more team member".ToQuantity(currentRound.TeamSize - currentTeam.TeamMembers.Count, ShowQuantityAs.Words));
                }
                else
                {
                    if (currentTeam.Vote.Count < GameSize)
                    {
                        //currentTeam.WaitingMessage = String.Format("Waiting for {0} to vote for {1}'s team", CommaQuibbling(game.Players.Select(p => p.Name).Except(currentTeam.Vote.Select(v => v.Player))), currentTeam.Leader);
                    }
                    else
                    {
                        if (currentTeam.QuestCards.Count < currentRound.TeamSize)
                        {
                            //currentTeam.WaitingMessage = String.Format("Waiting for {0} to quest", CommaQuibbling(currentTeam.TeamMembers.Except(game.CurrentRound.CurrentTeam.Quests.Select(q => q.Player.Name))));
                        }
                    }
                }
            }
		}

        public string PlayerName { get; set; }

        public string CommaQuibbling(IEnumerable<string> items)
        {
            var itemArray = items.ToArray();

            var commaSeparated = String.Join(", ", itemArray, 0, Math.Max(itemArray.Length - 1, 0));
            if (commaSeparated.Length > 0) commaSeparated += " and ";

            return commaSeparated + itemArray.LastOrDefault();
        }

        public Player AssassinsGuessAtMerlin { get; set; }
        public bool AssassinIsInTheGame { get; set; }
    }
}