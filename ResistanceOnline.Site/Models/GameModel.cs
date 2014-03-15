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
	public class GameModel
	{
		public int GameId { get; set; }
		public string State { get; set; }

        public bool GameOver
        {
            get
            {

                return State == Game.State.EternalChaos.ToString() || State == Game.State.EvilTriumphs.ToString() || State == Game.State.GoodPrevails.ToString();
            }
        }

        public SelectList LadyOfTheLakePlayerSelectList { get; set; }
        public SelectList GuessMerlinPlayersSelectList { get; set; }
        public SelectList AddToTeamPlayersSelectList { get; set; }
        public SelectList RemoveFromTeamSelectList { get; set; }
        public SelectList UseExcaliburSelectList { get; set; }
        public SelectList AssignExcaliburSelectList { get; set; }
        public SelectList AddRulesSelectList { get; set; }
        public SelectList RemoveRuleSelectList { get; set; }
        public SelectList AddCharacterCardsSelectList { get; set; }
        public SelectList RemoveCharacterCardSelectList { get; set; }

        public string PlayerName { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }
		public List<string> Actions { get; set; }
		public string WaitingMessage { get; set; }
		public List<PlayerInfoModel> PlayerInfo { get; set; }
		public List<RoundModel> Rounds { get; set; }
        public List<string> RoundTables { get; set; }
        public List<string> LoyaltyCardsDeltInAdvance { get; set; }
		public bool IsSpectator { get; set; }
        public int GameSize { get; set; }
        public List<string> Rules { get; set; }
        public string GameState { get; set; }
        public string PlayerCountSummary { get; set; }
        public string GameOverMessage { get; set; }
        public string Characters { get; set; }
        public string GameOwner { get; set; }
        public bool IsOwner { get; set; }

		public GameModel(int gameId, Game game, Guid playerGuid)
		{
            GameId = gameId;
            GameOwner = game.Players.First().Name;
            IsOwner = game.Players.First().Guid == playerGuid;
            PlayerCountSummary = ("player".ToQuantity(game.Players.Count));
            GameState = game.GameState.Humanize(LetterCasing.Sentence).ToString();
            GameSize = game.Players.Count;
            Rules = game.Rules.Select(r => r.Humanize()).ToList();
            RoundTables = game.RoundTables.Select(t=>String.Format("Quest {0} has {1} and requires {2}", (game.RoundTables.IndexOf(t) + 1).ToWords(), "player".ToQuantity(t.TeamSize, ShowQuantityAs.Words), "fail".ToQuantity(t.RequiredFails, ShowQuantityAs.Words))).ToList();
            LoyaltyCardsDeltInAdvance = new List<string>();
            if (game.Rules.Contains(Rule.LoyaltyCardsAreDeltInAdvance))
            {
                for (int i = 0; i < 4; i++)
                {
                    LoyaltyCardsDeltInAdvance.Add(string.Format("After quest {0}, Lancelot's {1} switch alegiance", (i + 1).ToWords(), game.LoyaltyDeck[i] == LoyaltyCard.SwitchAlegiance ? "will" : "will not"));
                }
            }

            var characters = new List<string>();
            foreach (var character in game.CharacterCards.Distinct()) {
                var count = game.CharacterCards.Count(c=>c==character);
                if (count > 1)
                {
                    switch (character)
                    {
                        case Character.LoyalServantOfArthur:
                            characters.Add(String.Format("{0} Loyal Servants of Arthur", count));
                            break;
                        case Character.MinionOfMordred:
                            characters.Add(String.Format("{0} Minions of Mordred", count));
                            break;
                        default:
                            characters.Add(character.Humanize().ToQuantity(count));
                            break;
                    }
                        
                } else {
                    characters.Add(character.Humanize());
                }
            }
            Characters = Useful.CommaQuibbling(characters);

            var player = game.Players.FirstOrDefault(p => p.Guid == playerGuid);
			IsSpectator = player == null;
            PlayerName = player == null ? "Spectator" : player.Name;

            AssassinsGuessAtMerlin = game.AssassinsGuessAtMerlin;
			State = game.GameState.ToString();

            var availableActions = game.AvailableActions(player);

            var guessMerlin = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.GuessMerlin);
            if (guessMerlin != null)
            {
                GuessMerlinPlayersSelectList = new SelectList(guessMerlin.ActionItems);
            }

            var ladyOfTheLake = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.UseTheLadyOfTheLake);
            if (ladyOfTheLake != null)
            {
                LadyOfTheLakePlayerSelectList = new SelectList(ladyOfTheLake.ActionItems);
            }

            var useExcalibur = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.UseExcalibur);
            if (useExcalibur != null)
            {
                UseExcaliburSelectList = new SelectList(useExcalibur.ActionItems);
            }

            var assignExcalibur = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.AssignExcalibur);
            if (assignExcalibur != null)
            {
                AssignExcaliburSelectList = new SelectList(assignExcalibur.ActionItems);
            }

            var addToTeam = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.AddToTeam);
            if (addToTeam != null)
            {
                AddToTeamPlayersSelectList = new SelectList(addToTeam.ActionItems);
            }
            var removeFromTeam = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.RemoveFromTeam);
            if (removeFromTeam != null)
            {
                RemoveFromTeamSelectList = new SelectList(removeFromTeam.ActionItems);
            }

            var addRule = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.AddRule);
            if (addRule != null)
            {
                AddRulesSelectList = new SelectList(addRule.ActionItems);
            }

            var removeRule = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.RemoveRule);
            if (removeRule != null)
            {
                RemoveRuleSelectList = new SelectList(removeRule.ActionItems);
            }

            var addCharacterCard = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.AddCharacterCard);
            if (addCharacterCard != null)
            {
                AddCharacterCardsSelectList = new SelectList(addCharacterCard.ActionItems);
            }

            var removeCharacterCard = availableActions.FirstOrDefault(a => a.ActionType == Core.Action.Type.RemoveCharacterCard);
            if (removeCharacterCard != null)
            {
                RemoveCharacterCardSelectList = new SelectList(removeCharacterCard.ActionItems);
            }


			Actions = game.AvailableActions(player).Select(i => i.ActionType.ToString()).ToList();         

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
                if (p == player || GameOver)
                {
                    playerInfo.CharacterCard = p.Character;
                    playerInfo.Knowledge = p.Character.ToString();
                }

				PlayerInfo.Add(playerInfo);

                if (p != player)
                {
                    waiting.AddRange(game.AvailableActions(p).Select(a => new WaitingActionsModel { Action = a.ActionType, Name = p.Name }));
                }
			}

            //build waiting message
            var waitings = waiting.Where(a=>a.Action != Core.Action.Type.Message).Select(w => w.Name).Distinct().ToList();
            if (waitings.Any())
            {
                WaitingMessage = String.Format("Waiting for {0}", Useful.CommaQuibbling(waitings));
            }
			
			Rounds = new List<RoundModel>();
			for(int i=0; i<game.Quests.Count; i++)
			{
                Rounds.Add(new RoundModel(game.Quests[i], i + 1, game, player, game.Players.Count));
			}

            if (GameOver)
            {
                GameOverMessage = String.Format("{0}, {1} win.", game.GameState.Humanize(LetterCasing.Sentence), Useful.CommaQuibbling(game.Winners.Select(p=>p.Name)));
            }
		}
    }
}