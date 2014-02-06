using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// this is essentially a state machine. State can be determined via the current game data and available actions for a person can be determined via the current state
    /// </summary>
    public class GameEngine
    {
        /// <summary>
        /// this is the available actions a player has given who they are and the state of the game
        /// this should show a list of buttons on the webpage or something
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<Action.Type> AvailableActions(Game game, Player player)
        {
            var gameState = game.DetermineState();
            switch (gameState)
            {
                case Game.State.Rounds:
                    var roundState = game.CurrentRound.DetermineState();
                    var quest = game.CurrentRound.CurrentQuest;
                    switch (roundState)
                    {
                        case Round.State.ProposingPlayers:
                            if (quest.Leader.Name == player.Name)
                            {
                                return new List<Action.Type>() { Action.Type.ProposePersonForQuest };
                            } 
                            return new List<Action.Type>();
                        case Round.State.Voting:
                            if (quest.Votes.Select(v => v.Player.Name).ToList().Contains(player.Name))
                            {
                                return new List<Action.Type>() { Action.Type.VoteForQuest };
                            } 
                            return new List<Action.Type>();
                        case Round.State.Questing:
                            if (quest.ProposedPlayers.Select(v => v.Name).ToList().Contains(player.Name) &&
                                !quest.QuestCards.Select(q=>q.Player.Name).ToList().Contains(player.Name))
                            {
                                return new List<Action.Type>() { Action.Type.SubmitQuestCard };
                            } 
                            return new List<Action.Type>();
                    }

                    return new List<Action.Type>();
                case Game.State.WaitingForCharacterSetup:
                    return new List<Action.Type>() { Action.Type.JoinGame, Action.Type.AddCharacterCard };
                case Game.State.WaitingForPlayers:
                    return new List<Action.Type>() { Action.Type.JoinGame };

                case Game.State.GuessingMerlin:
                    if (player.Character == Character.Assassin)
                        return new List<Action.Type>() { Action.Type.GuessMerlin };
                    return new List<Action.Type>();

                case Game.State.EvilTriumphs:
                case Game.State.GoodPrevails:
                case Game.State.MerlinDies:
                    return new List<Action.Type>();
            }
            return new List<Action.Type>();                

        }

        /// <summary>
        /// once a player performs an action, this should update the game state appropriately
        /// </summary>
        /// <param name="player"></param>
        /// <param name="action"></param>
        public void PerformAction(Game game, Player player, Action action)
        {
            if (!AvailableActions(game,player).Contains(action.ActionType))
                throw new Exception(String.Format("Hax. Player {0} can't perform action {1}", player.Name, action));

            throw new NotImplementedException();
        }


        /// <summary>
        /// does myself know someoneelse is evil. e.g. they are both minions, or myself is merlin
        /// </summary>
        /// <param name="playerSelf"></param>
        /// <param name="playerTarget"></param>
        /// <returns></returns>
        public static bool DetectEvil(Player myself, Player someoneelse)
        {
            //minions know each other (except oberon)
            if (myself.Character == Character.Assassin || myself.Character == Character.Morcana || myself.Character == Character.MinionOfMordred || myself.Character == Character.Mordred)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morcana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Mordred)
                {
                    return true;
                }
            }

            //merlin knows minions (except mordred)
            if (myself.Character == Character.Merlin)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morcana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Oberon)
                {
                    return true;
                }
            }  

            return false;
        }

        /// <summary>
        /// does myself know someoneelse is merlin (or morcana), e.g. myself is percival
        /// </summary>
        /// <param name="playerSelf"></param>
        /// <param name="playerTarget"></param>
        /// <returns></returns>
        public static bool DetectMerlin(Player myself, Player someoneelse)
        {
            if (myself.Character == Character.Percival)
            {
                if (someoneelse.Character == Character.Merlin || someoneelse.Character == Character.Morcana)
                {
                    return true;
                }
            }

            return false;
        }      
    }
}