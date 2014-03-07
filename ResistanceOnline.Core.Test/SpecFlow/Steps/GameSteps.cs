using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace ResistanceOnline.Core.Test.SpecFlow.Steps
{
	[Binding]
	public class GameSteps
	{
		readonly GameAssistant _gameAssistant;

		internal GameSteps(GameAssistant gameAssistant)
		{
			_gameAssistant = gameAssistant;
		}

		[Given(@"there is a standard game of (.*) players")]
		public void GivenThereIsAStandardGameOfPlayers(int numberOfPlayers)
		{
			_gameAssistant.CreateStandardGame(numberOfPlayers);
		}

		[Given(@"the game is in the (first|second|third|fourth|fifth) round")]
		public void GivenTheGameIsInTheFirstRound(string round)
		{
			var gameplay = ContextAccess.GamePlay;

			switch (round)
			{
				case "first":
					break;

				case "second":
					_gameAssistant.ProgressToRound(2);
					break;

				case "third":
					_gameAssistant.ProgressToRound(3);
					break;

				case "fourth":
					_gameAssistant.ProgressToRound(4);
					break;

				case "fifth":
					_gameAssistant.ProgressToRound(5);
					break;
			}
		}

		[Given(@"the leader has chosen his crew")]
		public void GivenTheLeaderHasChosenHisCrew()
		{
			_gameAssistant.ChooseCrew();
		}

		[Given(@"merlin is not a character")]
		public void GivenMerlinIsNotACharacter()
		{
			_gameAssistant.RemoveMerlin();
		}

		[Given(@"merlin is a character")]
		public void GivenMerlinIsACharacter()
		{
			_gameAssistant.AddMerlin();
		}

        [Given(@"assasin is a character")]
        public void GivenAssassinIsACharacter()
        {
            _gameAssistant.AddAssassin();
        }

		[When(@"everyone approves the quest")]
		public void WhenEveryoneApprovesTheQuest()
		{
			_gameAssistant.AllApproveTeam();
		}

		[When(@"everyone rejects the quest")]
		public void WhenEveryoneRejectsTheQuest()
		{
			_gameAssistant.AllDeclineTeam();
		}

		[When(@"(.*) players approve the quest")]
		public void WhenApproveTheQuest(int playersApprovingTheQuest)
		{
			_gameAssistant.VoteForTeam(playersApprovingTheQuest);
		}

		[When(@"the (first|second|third|fourth|fifth) quest is (successful|sabotaged)")]
		public void WhenQuestIsCompleted(string round, string result)
		{
			switch (round)
			{
				case "first":
					_gameAssistant.CompleteQuest(1, result == "successful");
					break;

				case "second":
					_gameAssistant.CompleteQuest(2, result == "successful");
					break;

				case "third":
					_gameAssistant.CompleteQuest(3, result == "successful");
					break;

				case "fourth":
					_gameAssistant.CompleteQuest(4, result == "successful");
					break;

				case "fifth":
					_gameAssistant.CompleteQuest(5, result == "successful");
					break;
			}
		}

        [Given(@"good have completed three quests")]
		[When(@"good have completed three quests")]
		public void GoodHaveCompletedThreeQuests()
		{
			for (var i = 1; i <= 3; i++ )
				_gameAssistant.CompleteQuest(i, true);
		}

		[When(@"the assasin fails to pick merlin")]
		public void TheAssasinFailsToPickMerlin()
		{
			_gameAssistant.PickMerlin(false);
		}

		[When(@"the assasin successfully picks merlin")]
		public void TheAssasinSuccessfullyPicksMerlin()
		{
			_gameAssistant.PickMerlin(true);
		}

		[Then(@"the quest goes ahead")]
		public void ThenTheQuestGoesAhead()
		{            
			Assert.AreEqual(ResistanceOnline.Core.GamePlay.State.Questing, ContextAccess.GamePlay.GamePlayState);
		}

		[Then(@"the quest does not go ahead")]
		public void ThenTheQuestDoesNotGoesAhead()
		{
            Assert.AreEqual(ResistanceOnline.Core.GamePlay.State.ChoosingTeam, ContextAccess.GamePlay.GamePlayState);
		}

		[Then(@"evil wins the game")]
		public void ThenEvilWinsTheGame()
		{
            Assert.AreEqual(ResistanceOnline.Core.GamePlay.State.EvilTriumphs, ContextAccess.GamePlay.GamePlayState);
		}

		[Then(@"good wins the game")]
		public void ThenGoodlWinsTheGame()
		{
            Assert.AreEqual(ResistanceOnline.Core.GamePlay.State.GoodPrevails, ContextAccess.GamePlay.GamePlayState);
        }

		[Then(@"merlin dies")]
		public void ThenMerlinDies()
		{
            Assert.AreEqual(ResistanceOnline.Core.GamePlay.State.EvilTriumphs, ContextAccess.GamePlay.GamePlayState);
        }

		[Then(@"assasin is to pick merlin")]
		public void AssasinIsToPickMerlin()
		{
            Assert.AreEqual(ResistanceOnline.Core.GamePlay.State.GuessingMerlin, ContextAccess.GamePlay.GamePlayState);
        }    
	}
}