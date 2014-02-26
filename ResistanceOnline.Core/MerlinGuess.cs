using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Core
{
    public class MerlinGuess
    {
        public Player Assassin { get; set; }
        public Player Guess { get; set; }
        public bool? MerlinDiesSuccess
        {
            get
            {
                if (Guess == null)
                    return null;
                return Guess.Character == Character.Merlin;
            }
        }
    }
}
