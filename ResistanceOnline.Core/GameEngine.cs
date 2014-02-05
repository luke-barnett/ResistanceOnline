using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class GameEngine
    {
        /// <summary>
        /// once all players have joined the game, this function allocates the characters to them
        /// </summary>
        public static void AllocateCharacters(Game game, List<Character> characters)
        {
            if (characters.Count != game.Players.Count)
                throw new Exception(String.Format("Number of characters {0} does not match player count {1}", characters.Count, game.Players.Count));
            
            Random random = new Random();
            foreach (var player in game.Players)
            {
                var roleIndex = random.Next(characters.Count);
                player.Character = characters[roleIndex];
                characters.RemoveAt(roleIndex);
            }
        }

        /// <summary>
        /// this is the available actions a player has given who they are and the state of the game
        /// this should show a list of buttons on the webpage or something
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<Action> AvailableActions(Game game, Player player)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// once a player performs an action, this should update the game state appropriately
        /// </summary>
        /// <param name="player"></param>
        /// <param name="action"></param>
        public void PerformAction(Game game, Player player, Action action)
        {
            if (!AvailableActions(game,player).Contains(action))
                throw new Exception(String.Format("Hax. Player {0} can't perform action {1} in game {2}", player.Name, action, game.GameId));

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