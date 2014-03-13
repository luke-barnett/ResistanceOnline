﻿using ResistanceOnline.Core;
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

		public Guid? PlayerGuid { get; set; }

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
        public SelectList UseExcaliburSelectList { get; set; }
        public SelectList AssignExcaliburSelectList { get; set; }
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

		public GameModel(Game game, Guid? playerGuid)
		{
            PlayerCountSummary = ("player".ToQuantity(game.Players.Count, ShowQuantityAs.Words)).ApplyCase(LetterCasing.Sentence);
            GameState = game.GameState.ToString();
			PlayerGuid = playerGuid;
            GameSize = game.Players.Count;
            Rules = game.Rules.Select(r => r.Humanize()).ToList();
            RoundTables = game.RoundTables.Select(t=>String.Format("Quest {0} has {1} and requires {2}", (game.RoundTables.IndexOf(t) + 1).ToWords(), "player".ToQuantity(t.TeamSize, ShowQuantityAs.Words), "fail".ToQuantity(t.RequiredFails, ShowQuantityAs.Words))).ToList();
            LoyaltyCardsDeltInAdvance = new List<string>();
            if (game.Rules.Contains(Rule.LoyaltyCardsAreDeltInAdvance) && game.ContainsLancelot())
            {
                for (int i = 0; i < 4; i++)
                {
                    LoyaltyCardsDeltInAdvance.Add(string.Format("After quest {0}, Lancelot's {1} switch alegiance", (i + 1).ToWords(), game.LoyaltyDeck[i] == LoyaltyCard.SwitchAlegiance ? "will" : "will not"));
                }
            }

            var characters = new List<string>();
            foreach (var character in game.AvailableCharacters.Distinct()) {
                var count = game.AvailableCharacters.Count(c=>c==character);
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
		
            //can guess anyone but self
            GuessMerlinPlayersSelectList = new SelectList(game.Players.Where(p => p != player).Select(p => p.Name));

            //can use on anyone who hasn't had it
            var ladyOfTheLakeHistory = game.Quests.Where(r=>r.LadyOfTheLake!=null).Select(r => r.LadyOfTheLake.Holder);
            LadyOfTheLakePlayerSelectList = new SelectList(game.Players.Where(p => p != player).Except(ladyOfTheLakeHistory).Select(p => p.Name));

            if (game.CurrentQuest != null && game.CurrentQuest.CurrentVoteTrack != null)
            {
                UseExcaliburSelectList = new SelectList(game.CurrentQuest.CurrentVoteTrack.Players.Where(p => p != game.CurrentQuest.CurrentVoteTrack.Leader).Select(p => p.Name));
                AssignExcaliburSelectList = new SelectList(game.CurrentQuest.CurrentVoteTrack.Players.Select(p => p.Name));
            }

            //can put anyone on a team who isn't already on it
            if (game.CurrentQuest != null)
            {
                AddToTeamPlayersSelectList = new SelectList(game.Players.Where(p => !game.CurrentQuest.CurrentVoteTrack.Players.Select(t => t.Name).ToList().Contains(p.Name)).Select(p => p.Name));
            }

			Actions = game.AvailableActions(player).Select(i => i.ToString()).ToList();         

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

				waiting.AddRange(game.AvailableActions(p).Select(a => new WaitingActionsModel { Action = a, Name = p.Name }));
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
                GameOverMessage = game.GameState.Humanize(LetterCasing.Sentence);
            }
		}


    }
}