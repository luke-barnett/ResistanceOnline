using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core.Test
{
	[TestClass]
	public class RoundTest
	{
		[TestMethod]
		public void EasySuccess()
		{
			var players = new List<Player> { new Player(), new Player(), new Player(), new Player(), new Player() };
			var round = new Round(players, 0, 3, 1);

			//to start with we should be waiting for someone to choose the players
			Assert.AreEqual(Round.State.ProposingPlayers, round.DetermineState());

			//select the players for the quest
			round.ProposePlayer(players[0]);
			Assert.AreEqual(Round.State.ProposingPlayers, round.DetermineState());
			round.ProposePlayer(players[1]);
			round.ProposePlayer(players[2]);

			Assert.AreEqual(Round.State.Voting, round.DetermineState());

			//do some voting to see if everyone approves
			round.VoteForQuest(players[0], true);
			Assert.AreEqual(Round.State.Voting, round.DetermineState());
			round.VoteForQuest(players[1], true);
			round.VoteForQuest(players[2], false);
			round.VoteForQuest(players[3], false);
			round.VoteForQuest(players[4], true);
			
			//do the quest
			Assert.AreEqual(Round.State.Questing, round.DetermineState());
			round.SubmitQuest(players[0], true);
			Assert.AreEqual(Round.State.Questing, round.DetermineState());
			round.SubmitQuest(players[1], true);
			round.SubmitQuest(players[2], true);

			Assert.AreEqual(Round.State.Succeeded, round.DetermineState());
		}

		[TestMethod]
		public void FailedRound()
		{
			var players = new List<Player> { new Player(), new Player(), new Player(), new Player(), new Player() };
			var round = new Round(players, 0, 3, 1);

			//to start with we should be waiting for someone to choose the players
			Assert.AreEqual(Round.State.ProposingPlayers, round.DetermineState());

			//select the players for the quest
			round.ProposePlayer(players[0]);
			Assert.AreEqual(Round.State.ProposingPlayers, round.DetermineState());
			round.ProposePlayer(players[1]);
			round.ProposePlayer(players[2]);

			Assert.AreEqual(Round.State.Voting, round.DetermineState());

			//do some voting to see if everyone approves
			round.VoteForQuest(players[0], true);
			Assert.AreEqual(Round.State.Voting, round.DetermineState());
			round.VoteForQuest(players[1], false);
			round.VoteForQuest(players[2], false);
			round.VoteForQuest(players[3], false);
			round.VoteForQuest(players[4], true);

			//nope, try again
			Assert.AreEqual(Round.State.ProposingPlayers, round.DetermineState());
			round.ProposePlayer(players[2]);
			Assert.AreEqual(Round.State.ProposingPlayers, round.DetermineState());
			round.ProposePlayer(players[3]);
			round.ProposePlayer(players[4]);

			Assert.AreEqual(Round.State.Voting, round.DetermineState());

			//do some voting to see if everyone approves
			round.VoteForQuest(players[0], true);
			Assert.AreEqual(Round.State.Voting, round.DetermineState());
			round.VoteForQuest(players[1], true);
			round.VoteForQuest(players[2], false);
			round.VoteForQuest(players[3], false);
			round.VoteForQuest(players[4], true);

			//do the quest
			Assert.AreEqual(Round.State.Questing, round.DetermineState());
			round.SubmitQuest(players[0], false);
			Assert.AreEqual(Round.State.Questing, round.DetermineState());
			round.SubmitQuest(players[1], false);
			round.SubmitQuest(players[2], true);

			Assert.AreEqual(Round.State.Failed, round.DetermineState());
		}

	}
}
