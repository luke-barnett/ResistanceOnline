using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// this could probably be better as a base class with classes for different actions
    /// </summary>
    public class Action
    {
        public enum Type
        {
            AddCharacter,
            JoinGame,
            AddToTeam,
            VoteForTeam,
            SubmitQuestCard,
            GuessMerlin,
            UseTheLadyOfTheLake,
            Message,
            AddBot
        };

        public Type ActionType { get; set; }

        /// <summary>
        /// only useful for proposing quests
        /// </summary>
        public Player Player { get; set; }

        /// <summary>
        /// only useful for Joining games
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// only usefule for sending messages
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///  only useful for Voting on quests
        /// </summary>
        public bool Accept { get; set; }

        /// <summary>
        /// only useful for submitting quest cards
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// only useful for adding characters to the game before in starts
        /// </summary>
        public Character Character { get; set; }
    }
}
