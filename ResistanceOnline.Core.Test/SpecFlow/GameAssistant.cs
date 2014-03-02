using System.Linq;
using System;
using TechTalk.SpecFlow;

namespace ResistanceOnline.Core.Test.SpecFlow
{
	internal class GameAssistant
	{
		internal void CreateStandardGame(int numberOfPlayers)
		{
			var game = new Game(new GameSetup());

			game.Setup.Rules.Remove(Rule.IncludeLadyOfTheLake);

			//TODO: map to standard game types for good/evil numbers
			for (var i = 0; i < numberOfPlayers; i++)
			{				
				game.Setup.JoinGame(string.Format("player{0}", i), Guid.NewGuid());
                game.Setup.SetCharacter(i, Character.LoyalServantOfArthur);
			}
            game.Setup.Rules.Clear();
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
            game.DoAction(game.Setup.GameId, leader, Action.Type.AddToTeam, leader);
            foreach (var player in game.Setup.Players.Where(player => player != leader).Take(game.CurrentRound.TeamSize - 1))
			{
                game.DoAction(game.Setup.GameId, leader, Action.Type.AddToTeam, player);
			}
		}

		internal void AllApproveTeam()
		{
            VoteForTeam(ContextAccess.Game.Setup.GameSize);
		}

		internal void AllDeclineTeam()
		{
			VoteForTeam(0);
		}

		internal void VoteForTeam(int numberOfPlayersForTeam)
		{
			var game = ContextAccess.Game;

            foreach (var player in ContextAccess.Game.Setup.Players.Take(numberOfPlayersForTeam))
			{
                game.DoAction(game.Setup.GameId, player, Action.Type.VoteApprove);
			}

            foreach (var player in ContextAccess.Game.Setup.Players.Skip(numberOfPlayersForTeam))
			{
                game.DoAction(game.Setup.GameId, player, Action.Type.VoteReject);
            }
		}

		internal void RemoveMerlin()
		{
			var game = ContextAccess.Game;
            foreach (var merlinPlayer in game.Setup.Players.Where(player => player.Character == Character.Merlin))
			{
				merlinPlayer.Character = Character.LoyalServantOfArthur;
			}

            foreach (var assasinPlayer in game.Setup.Players.Where(player => player.Character == Character.Assassin))
			{
				assasinPlayer.Character = Character.MinionOfMordred;
			}

            var merlinCount = game.Setup.AvailableCharacters.Count(character => character == Character.Merlin);

            game.Setup.AvailableCharacters.RemoveAll(character => character == Character.Merlin);

			for (var i = 0; i < merlinCount; i++)
                game.Setup.AvailableCharacters.Add(Character.LoyalServantOfArthur);

            var assasinCount = game.Setup.AvailableCharacters.Count(character => character == Character.Assassin);

            game.Setup.AvailableCharacters.RemoveAll(character => character == Character.Assassin);

			for (var i = 0; i < assasinCount; i++)
                game.Setup.AvailableCharacters.Add(Character.MinionOfMordred);
		}

		internal void CompleteQuest(int roundNumber, bool successful)
		{
            var game = ContextAccess.Game;

            //build team
            for (int i = 0; i < game.CurrentRound.TeamSize; i++)
            {
                game.DoAction(game.Setup.GameId, game.CurrentRound.CurrentTeam.Leader, Action.Type.AddToTeam, game.Setup.Players[i]);
            }

            //pass the vote
            foreach (var player in game.Setup.Players)
            {
                game.DoAction(game.Setup.GameId, player, Action.Type.VoteApprove);
            }

            //do the quest
            foreach (var player in game.CurrentRound.CurrentTeam.TeamMembers)
            {
                game.DoAction(game.Setup.GameId, player, successful ? Action.Type.SucceedQuest : Action.Type.FailQuest);
            }
        }

		internal void AddMerlin()
		{
            var game = ContextAccess.Game;
            game.Setup.Players.First(p => p.Character == Character.LoyalServantOfArthur).Character = Character.Merlin;
            game.Setup.AvailableCharacters[game.Setup.AvailableCharacters.IndexOf(Character.LoyalServantOfArthur)] = Character.Merlin;
		}

        internal void AddAssassin()
        {
            var game = ContextAccess.Game;
            game.Setup.Players.First(p => p.Character == Character.LoyalServantOfArthur).Character = Character.Assassin;
            game.Setup.AvailableCharacters[game.Setup.AvailableCharacters.IndexOf(Character.LoyalServantOfArthur)] = Character.Assassin;
        }

		internal void PickMerlin(bool successfullMerlinPick)
		{
			var game = ContextAccess.Game;

            var assasin = game.Setup.Players.First(player => player.Character == Character.Assassin);

			if (successfullMerlinPick)
			{
                game.DoAction(game.Setup.GameId, assasin, Action.Type.GuessMerlin, game.Setup.Players.First(player => player.Character == Character.Merlin));
			}
			else
			{
                game.DoAction(game.Setup.GameId, assasin, Action.Type.GuessMerlin, game.Setup.Players
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