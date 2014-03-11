using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Humanizer;

namespace ResistanceOnline.Site.Models
{
    public class GameModel
    {
        public List<string> Players { get; set; }
        public int GameId { get; set; }
        public List<Character> AvailableCharacters { get; set; }
        public List<Rule> Rules { get; set; }
        public List<QuestSize> RoundTables;
        public string State { get; set; }

        public GameModel(Game game)
        {
            Players = game.Players.Select(p=>p.Name).ToList();
            GameId = game.GameId;
            AvailableCharacters = game.AvailableCharacters;
            Rules = game.Rules;
            RoundTables = game.RoundTables;
            State = game.GameState.ToString();
        }
    }
}