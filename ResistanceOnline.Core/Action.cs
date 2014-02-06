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
            JoinGame,
            ProposePersonForQuest,
            VoteForQuest,
            SubmitQuestCard,
            GuessMerlin
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
        ///  only useful for Voting on quests
        /// </summary>
        public bool Accept { get; set; }

        /// <summary>
        /// only useful for submitting quest cards
        /// </summary>
        public bool Success { get; set; }
    }
}
