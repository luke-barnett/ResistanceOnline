using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using TechTalk.SpecFlow;

namespace ResistanceOnline.Core.Test.SpecFlow.Steps
{
	[Binding]
	public class GameSteps
	{
		[Given(@"there is a standard game of (.*) players")]
		public void GivenThereIsAStandardGameOfPlayers(int numberOfPlayers)
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

		[Given(@"the game is in the (first|second|third|fourth|fifth) round")]
		public void GivenTheGameIsInTheFirstRound(string round)
		{
			var game = ContextAccess.Game;

			switch (round)
			{
				case "first":
					break;

				case "second":
					ScenarioContext.Current.Pending();
					break;

				case "third":
					ScenarioContext.Current.Pending();
					break;

				case "fourth":
					ScenarioContext.Current.Pending();
					break;

				case "fifth":
					ScenarioContext.Current.Pending();
					break;
			}
		}

		[Given(@"the leader has chosen his crew")]
		public void GivenTheLeaderHasChosenHisCrew()
		{
			var game = ContextAccess.Game;
			var leader = game.CurrentRound.CurrentQuest.Leader;
			game.CurrentRound.PutOnQuest(leader, leader);
			foreach (var player in game.Players.Where(player => player != leader).Take(game.CurrentRound.Size - 1))
			{
				game.CurrentRound.PutOnQuest(leader, player);
			}
		}

		[When(@"everyone approves the quest")]
		public void WhenAllPlayersApproveTheQuest()
		{
			var round = ContextAccess.Game.CurrentRound;

			foreach (var player in ContextAccess.Game.Players)
			{
				round.VoteForQuest(player, true);
			}
		}

		[When(@"(.*) players approve the quest")]
		public void WhenApproveTheQuest(int playersApprovingTheQuest)
		{
			var round = ContextAccess.Game.CurrentRound;

			foreach (var player in ContextAccess.Game.Players.Take(playersApprovingTheQuest))
			{
				round.VoteForQuest(player, true);
			}

			foreach (var player in ContextAccess.Game.Players.Skip(playersApprovingTheQuest))
			{
				round.VoteForQuest(player, false);
			}
		}

		[Then(@"the quest goes ahead")]
		public void ThenTheQuestGoesAhead()
		{
			Assert.AreEqual(ResistanceOnline.Core.Round.State.Questing, ContextAccess.Game.CurrentRound.DetermineState());
		}

		[Then(@"the quest does not goes ahead")]
		public void ThenTheQuestDoesNotGoesAhead()
		{
			Assert.AreEqual(ResistanceOnline.Core.Round.State.ProposingPlayers, ContextAccess.Game.CurrentRound.DetermineState());
		}
	}
}