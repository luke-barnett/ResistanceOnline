using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ResistanceOnline.Core.Test
{
    [TestClass]
    public class GameEngineTest
    {
        [TestMethod]
        public void DetectEvil()
        {
            EvilTest(Character.Assassin, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morcana, Character.Mordred });
            EvilTest(Character.MinionOfMordred, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morcana, Character.Mordred });
            EvilTest(Character.Morcana, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morcana, Character.Mordred });
            EvilTest(Character.Mordred, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morcana, Character.Mordred });
            EvilTest(Character.Oberon, new List<Character> { });
            EvilTest(Character.LoyalServantOfArthur, new List<Character> { });
            EvilTest(Character.Percival, new List<Character> { });
            EvilTest(Character.Merlin, new List<Character> { Character.Assassin, Character.MinionOfMordred, Character.Morcana, Character.Oberon });
        }

        private void EvilTest(Character character, List<Character> expected) 
        {
            var player = new Player { Character = character };
            var someoneElse = new Player();
            foreach (var c in Enum.GetValues(typeof(Character)))
            {
                someoneElse.Character = (Character)c;
                var result = GameEngine.DetectEvil(player, someoneElse);

                Assert.AreEqual(expected.Contains((Character)c), result, String.Format("{0} thinks {1} is {2}", character, c, result ? "Good" : "Evil"));
            }
        }

        [TestMethod]
        public void DetectMerlin()
        {
            MerlinTest(Character.Assassin, new List<Character> {  });
            MerlinTest(Character.MinionOfMordred, new List<Character> {  });
            MerlinTest(Character.Morcana, new List<Character> {  });
            MerlinTest(Character.Mordred, new List<Character> {  });
            MerlinTest(Character.Oberon, new List<Character> { });
            MerlinTest(Character.LoyalServantOfArthur, new List<Character> { });
            MerlinTest(Character.Percival, new List<Character> { Character.Morcana, Character.Merlin });
            MerlinTest(Character.Merlin, new List<Character> {  });
        }

        private void MerlinTest(Character character, List<Character> expected)
        {
            var player = new Player { Character = character };
            var someoneElse = new Player();
            foreach (var c in Enum.GetValues(typeof(Character)))
            {
                someoneElse.Character = (Character)c;
                var result = GameEngine.DetectMerlin(player, someoneElse);

                Assert.AreEqual(expected.Contains((Character)c), result, String.Format("{0} thinks {1} is {2}", character, c, result ? "Not Merlin" : "Merlin"));
            }
        }
    }
}
