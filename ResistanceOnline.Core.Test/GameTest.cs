using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ResistanceOnline.Core.Test
{
    [TestClass]
    public class GameTest
    {
        [TestMethod]
        public void JoinAndAllocate()
        {
            var game = new Game(5);

            Assert.AreEqual(Game.State.WaitingForCharacterSetup, game.DetermineState());
            game.AddCharacter(Character.Assassin);
            game.AddCharacter(Character.LoyalServantOfArthur);
            game.AddCharacter(Character.Percival);
            game.AddCharacter(Character.Morcana);
            game.AddCharacter(Character.Merlin);

            Assert.AreEqual(Game.State.WaitingForPlayers, game.DetermineState());
            game.JoinGame("a");
            game.JoinGame("b");
            game.JoinGame("c");
            game.JoinGame("d");
            Assert.AreEqual(Game.State.WaitingForPlayers, game.DetermineState());
            game.JoinGame("e");
            Assert.AreEqual(Game.State.Rounds, game.DetermineState());

            var allocatedCharacters = game.Players.Select(p=> p.Character).ToList();
            Assert.IsTrue(allocatedCharacters.Contains(Character.Assassin));
            Assert.IsTrue(allocatedCharacters.Contains(Character.LoyalServantOfArthur));
            Assert.IsTrue(allocatedCharacters.Contains(Character.Percival));
            Assert.IsTrue(allocatedCharacters.Contains(Character.Morcana));
            Assert.IsTrue(allocatedCharacters.Contains(Character.Merlin));
            Assert.IsFalse(allocatedCharacters.Contains(Character.UnAllocated));
        }        
    }
}
