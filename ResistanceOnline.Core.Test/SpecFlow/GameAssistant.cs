using System.Linq;
using TechTalk.SpecFlow;

namespace ResistanceOnline.Core.Test.SpecFlow
{
	internal class GameAssistant
	{
		internal void CreateStandardGame(int numberOfPlayers)
		{
			var game = new Game(numberOfPlayers, false);

			//TODO: map to standard game types for good/evil numbers
			for (var i = 0; i < numberOfPlayers; i++)
			{
				game.AddCharacter(Character.LoyalServantOfArthur);
				game.JoinGame(string.Format("player{0}", i));
			}

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
			var leader = game.CurrentRound.CurrentQuest.Leader;
			game.CurrentRound.PutOnQuest(leader, leader);
			foreach (var player in game.Players.Where(player => player != leader).Take(game.CurrentRound.Size - 1))
			{
				game.CurrentRound.PutOnQuest(leader, player);
			}
		}

		internal void AllApproveTeam()
		{
			VoteForTeam(ContextAccess.Game.TotalPlayers);
		}

		internal void AllDeclineTeam()
		{
			VoteForTeam(0);
		}

		internal void VoteForTeam(int numberOfPlayersForTeam)
		{
			var round = ContextAccess.Game.CurrentRound;

			foreach (var player in ContextAccess.Game.Players.Take(numberOfPlayersForTeam))
			{
				round.VoteForQuest(player, true);
			}

			foreach (var player in ContextAccess.Game.Players.Skip(numberOfPlayersForTeam))
			{
				round.VoteForQuest(player, false);
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
			ScenarioContext.Current.Pending();
		}

		internal void AddMerlin()
		{
			ScenarioContext.Current.Pending();
		}

		internal void PickMerlin(bool successfullMerlinPick)
		{
			var game = ContextAccess.Game;

			var assasin = game.Players.First(player => player.Character == Character.Assassin);

			if (successfullMerlinPick)
			{
				game.PerformAction(assasin, new Action { ActionType = Action.Type.GuessMerlin, Player = game.Players.First(player => player.Character == Character.Merlin) });
			}
			else
			{
				game.PerformAction(assasin,
					new Action
					{
						ActionType = Action.Type.GuessMerlin,
						Player = game.Players
							.Where(player => player.Character == Character.LoyalServantOfArthur || player.Character == Character.Percival)
							.First(player => player.Character != Character.Merlin)
					});
			}
		}

		void IncrementRound()
		{
			ScenarioContext.Current.Pending();

			//Based on round number run CompleteQuest(roundNumber, successful)
		}
	}
}