using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ResistanceOnline.Site.Models
{
    public class GameViewModel
    {
        public class PlayerInfoModel
        {
            public string Name { get; set; }
            public bool IsEvil { get; set; }
            public bool CouldBeMerlin { get; set; }
            public Character? CharacterCard { get; set; }
        }

        public class OtherActions {
            public string Name { get; set; }
            public string Action { get; set; }
        }

        public int GameId { get; set; }

        public Guid? PlayerGuid { get; set; }

        public Core.Game.State GameState { get; set; }
        
        public List<Core.Player> ImpersonationList { get; set; }

        public List<Core.Character> CharactersInGame { get; set; }

        public System.Web.Mvc.SelectList AllCharactersSelectList { get; set; }

        public SelectList PlayersSelectList { get; set; }

        public List<Core.Action.Type> Actions { get; set; }

        public List<OtherActions> Waiting { get; set; }

        public List<PlayerInfoModel> PlayerInfo { get; set; }

        public bool IsSpectator { get; set; }

        public GameViewModel(Game game, Guid? playerGuid)
        {
            GameId = game.GameId;
            PlayerGuid = playerGuid;

            var player = game.Players.FirstOrDefault(p => p.Guid == playerGuid);
            IsSpectator = player == null;

            if (game.ImpersonationEnabled)
            {
                ImpersonationList = game.Players.ToList();
            }

            GameState = game.DetermineState();
            CharactersInGame = game.AvailableCharacters.ToList();
            AllCharactersSelectList = new SelectList(Enum.GetNames(typeof(Character)).Where(c => c != Character.UnAllocated.ToString()).ToList());
            PlayersSelectList = new SelectList(game.Players.Select(p => p.Name));
            if (player != null)
            {
                Actions = game.AvailableActions(player);
            }

            PlayerInfo = new List<PlayerInfoModel>();
            Waiting = new List<OtherActions>();
            foreach (var p in game.Players)
            {
                var playerInfo = new PlayerInfoModel 
                { 
                    CharacterCard = p==player? p.Character : (Character?)null, 
                    Name = p.Name, 
                    CouldBeMerlin = Game.DetectMerlin(player, p), 
                    IsEvil = Game.DetectEvil(player, p) 
                };
                PlayerInfo.Add(playerInfo);

                Waiting.AddRange(game.AvailableActions(p).Select(a => new OtherActions { Action = a.ToString(), Name = p.Name }));
            }
            
            //game history
            var log = new List<string>();
            foreach (var round in game.Rounds)
            {
                log.Add("Round " + game.Rounds.IndexOf(round) + " - " + round.DetermineState().ToString());
                log.Add("Round size " + round.Size);
                log.Add("Required fails " + round.RequiredFails);
                foreach (var quest in round.Quests)
                {
                    log.Add("Quest " + round.Quests.IndexOf(quest));
                    log.Add("Quest Leader: " + quest.Leader.Name);
                    foreach (var p in quest.ProposedPlayers)
                    {
                        log.Add("Proposed player: " + p.Name);
                    }
                    foreach (var v in quest.Votes)
                    {
                        log.Add(v.Player.Name + " votes " + (quest.Votes.Count == round.TotalPlayers ? (v.Approve ? "Approve" : "Reject") : "submitted"));
                    }
                    if (quest.QuestCards.Count == round.Size)
                    {
                        foreach (var q in quest.QuestCards.Select(q => q.Success).OrderBy(q => q))
                        {
                            log.Add(q ? "Success" : "Fail");
                        }
                    }
                    else
                    {
                        foreach (var q in quest.QuestCards)
                        {
                            log.Add(q.Player.Name + " has submitted quest card");
                        }

                    }
                }
            }
            Log = log;


        }


        public List<string> Log { get; set; }
    }
}