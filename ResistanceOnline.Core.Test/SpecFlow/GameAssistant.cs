using System.Linq;
using System;
using TechTalk.SpecFlow;

namespace ResistanceOnline.Core.Test.SpecFlow
{
	internal class GameAssistant
	{
		internal void CreateStandardGame(int numberOfPlayers)
		{
			var game = new Game();

			game.Rules.Remove(Rule.IncludeLadyOfTheLake);

			//TODO: map to standard game types for good/evil numbers
			for (var i = 0; i < numberOfPlayers; i++)
			{				
				game.JoinGame(string.Format("player{0}", i), Guid.NewGuid());
                game.SetCharacter(i, Character.LoyalServantOfArthur);
			}
            game.StartGame();

			ContextAccess.Game = game;
		}

		internal void ProgressToRound(int round)
		{
			for (var i = 1; i < round; i++)
			{
				IncrementRound();
			}
		}

		internal void ChooseCrew()
		{
			var game = ContextAccess.Game;
			var leader = game.CurrentRound.CurrentTeam.Leader;
			game.AddToTeam(leader, leader);
			foreach (var player in game.Players.Where(player => player != leader).Take(game.CurrentRound.TeamSize - 1))
			{
				game.AddToTeam(leader, player);
			}
		}

		internal void AllApproveTeam()
		{
			VoteForTeam(ContextAccess.Game.GameSize);
		}

		internal void AllDeclineTeam()
		{
			VoteForTeam(0);
		}

		internal void VoteForTeam(int numberOfPlayersForTeam)
		{
			var game = ContextAccess.Game;

			foreach (var player in ContextAccess.Game.Players.Take(numberOfPlayersForTeam))
			{
				game.VoteForTeam(player, true);
			}

			foreach (var player in ContextAccess.Game.Players.Skip(numberOfPlayersForTeam))
			{
                game.VoteForTeam(player, false);
			}
		}

		internal void RemoveMerlin()
		{
			var game = ContextAccess.Game;
			foreach (var merlinPlayer in game.Players.Where(player => player.Character == Character.Merlin))
			{
				merlinPlayer.Character = Character.LoyalServantOfArthur;
			}

			foreach (var assasinPlayer in game.Players.Where(player => player.Character == Character.Assassin))
			{
				assasinPlayer.Character = Character.MinionOfMordred;
			}

			var merlinCount = game.AvailableCharacters.Count(character => character == Character.Merlin);

			game.AvailableCharacters.RemoveAll(character => character == Character.Merlin);

			for (var i = 0; i < merlinCount; i++)
				game.AvailableCharacters.Add(Character.LoyalServantOfArthur);

			var assasinCount = game.AvailableCharacters.Count(character => character == Character.Assassin);

			game.AvailableCharacters.RemoveAll(character => character == Character.Assassin);

			for (var i = 0; i < assasinCount; i++)
				game.AvailableCharacters.Add(Character.MinionOfMordred);
		}

		internal void CompleteQuest(int roundNumber, bool successful)
		{
            var game = ContextAccess.Game;

            //build team
            for (int i = 0; i < game.CurrentRound.TeamSize; i++)
            {
                game.AddToTeam(game.CurrentRound.CurrentTeam.Leader, game.Players[i]);
            }

            //pass the vote
            foreach (var player in game.Players)
            {
                game.VoteForTeam(player, true);
            }

            //do the quest
            foreach (var player in game.CurrentRound.CurrentTeam.TeamMembers)
            {
                game.SubmitQuest(player, successful);
            }
        }

		internal void AddMerlin()
		{
            var game = ContextAccess.Game;
            game.Players.First(p => p.Character == Character.LoyalServantOfArthur).Character = Character.Merlin;
            game.AvailableCharacters[game.AvailableCharacters.IndexOf(Character.LoyalServantOfArthur)] = Character.Merlin;
		}

        internal void AddAssassin()
        {
            var game = ContextAccess.Game;
            game.Players.First(p => p.Character == Character.LoyalServantOfArthur).Character = Character.Assassin;
            game.AvailableCharacters[game.AvailableCharacters.IndexOf(Character.LoyalServantOfArthur)] = Character.Assassin;
        }

		internal void PickMerlin(bool successfullMerlinPick)
		{
			var game = ContextAccess.Game;

			var assasin = game.Players.First(player => player.Character == Character.Assassin);

			if (successfullMerlinPick)
			{
				game.GuessMerlin(assasin, game.Players.First(player => player.Character == Character.Merlin));
			}
			else
			{
                game.GuessMerlin(assasin, game.Players
                    .Where(player => player.Character == Character.LoyalServantOfArthur || player.Character == Character.Percival)
                    .First(player => player.Character != Character.Merlin));
			}
		}

		void IncrementRound()
		{
			var game = ContextAccess.Game;
            var roundNumber = game.Rounds.Count;
            CompleteQuest(roundNumber, roundNumber % 2 == 0);
		}
	}
}