﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ResistanceOnline.Core.Test
{
    [TestClass]
    public class GameTest
    {
        [TestMethod]
        public void DetectEvil()
        {
            EvilTest(Character.Assassin, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morgana, Character.Mordred, Character.EvilLancelot });
            EvilTest(Character.MinionOfMordred, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morgana, Character.Mordred, Character.EvilLancelot });
            EvilTest(Character.Morgana, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morgana, Character.Mordred, Character.EvilLancelot });
            EvilTest(Character.Mordred, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morgana, Character.Mordred, Character.EvilLancelot });
            EvilTest(Character.Oberon, new List<Character> { });
            EvilTest(Character.LoyalServantOfArthur, new List<Character> { });
            EvilTest(Character.Percival, new List<Character> { });
            EvilTest(Character.Merlin, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morgana, Character.Oberon });
        }

        private void EvilTest(Character character, List<Character> expected)
        {
            var player = new Player { Character = character };
            var someoneElse = new Player();
            foreach (var c in Enum.GetValues(typeof(Character)))
            {
                someoneElse.Character = (Character)c;
                var result = Game.DetectEvil(player, someoneElse);

                Assert.AreEqual(expected.Contains((Character)c), result, String.Format("{0} thinks {1} is {2}", character, c, result ? "Good" : "Evil"));
            }
        }

        [TestMethod]
        public void DetectMerlin()
        {
            MerlinTest(Character.Assassin, new List<Character> { });
            MerlinTest(Character.MinionOfMordred, new List<Character> { });
            MerlinTest(Character.Morgana, new List<Character> { });
            MerlinTest(Character.Mordred, new List<Character> { });
            MerlinTest(Character.Oberon, new List<Character> { });
            MerlinTest(Character.LoyalServantOfArthur, new List<Character> { });
            MerlinTest(Character.Percival, new List<Character> { Character.Morgana, Character.Merlin });
            MerlinTest(Character.Merlin, new List<Character> { });
        }

        private void MerlinTest(Character character, List<Character> expected)
        {
            var player = new Player { Character = character };
            var someoneElse = new Player();
            foreach (var c in Enum.GetValues(typeof(Character)))
            {
                someoneElse.Character = (Character)c;
                var result = Game.DetectMerlin(player, someoneElse);

                Assert.AreEqual(expected.Contains((Character)c), result, String.Format("{0} thinks {1} is {2}", character, c, result ? "Not Merlin" : "Merlin"));
            }
        }

        [TestMethod]
        public void JoinAndAllocate()
        {
            var game = new Game(5);

            Assert.AreEqual(Game.State.GameSetup, game.DetermineState());
            game.AddCharacter(Character.Assassin);
            game.AddCharacter(Character.LoyalServantOfArthur);
            game.AddCharacter(Character.Percival);
            game.AddCharacter(Character.Morgana);
            game.AddCharacter(Character.Merlin);

            Assert.AreEqual(Game.State.GameSetup, game.DetermineState());
            game.JoinGame("a");
            game.JoinGame("b");
            game.JoinGame("c");
            game.JoinGame("d");
            Assert.AreEqual(Game.State.GameSetup, game.DetermineState());
            game.JoinGame("e");
            Assert.AreEqual(Game.State.Playing, game.DetermineState());

            var allocatedCharacters = game.Players.Select(p=> p.Character).ToList();
            Assert.IsTrue(allocatedCharacters.Contains(Character.Assassin));
            Assert.IsTrue(allocatedCharacters.Contains(Character.LoyalServantOfArthur));
            Assert.IsTrue(allocatedCharacters.Contains(Character.Percival));
            Assert.IsTrue(allocatedCharacters.Contains(Character.Morgana));
            Assert.IsTrue(allocatedCharacters.Contains(Character.Merlin));
            Assert.IsFalse(allocatedCharacters.Contains(Character.UnAllocated));
        }        
    }
}
