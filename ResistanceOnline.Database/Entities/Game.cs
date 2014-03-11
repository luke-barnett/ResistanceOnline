using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Database.Entities
{
	public class Game
	{
		public int GameId { get; set; }
        public string GameState { get; set; }
        public List<Character> Characters { get; set; }
        public List<Player> Players { get; set; }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }
        public List<Rule> Rules { get; set; }
        public List<Round> RoundTables { get; set; }
        public string InitialHolderOfLadyOfTheLake { get; set; }
        public string InitialLeader { get; set; }
	}
}
