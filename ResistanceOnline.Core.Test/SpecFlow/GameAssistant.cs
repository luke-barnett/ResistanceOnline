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

            game.Rules.Remove(Rule.LadyOfTheLakeExists);

			for (var i = 0; i < numberOfPlayers; i++)
			{
                game.JoinGame(string.Format("player{0}", i), Guid.NewGuid());
                game.AvailableCharacters[i] = Character.LoyalServantOfArthur;
			}
            game.Rules.Clear();
            game.StartGame();
            var gameplay = new GamePlay(game);
			ContextAccess.GamePlay = gameplay;
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
			var gameplay = ContextAccess.GamePlay;
			var leader = gameplay.CurrentQuest.CurrentVoteTrack.Leader;
            gameplay.DoAction(gameplay.Game.GameId, leader, Action.Type.AddToTeam, leader);
            foreach (var player in gameplay.Game.Players.Where(player => player != leader).Take(gameplay.CurrentQuest.TeamSize - 1))
			{
                gameplay.DoAction(gameplay.Game.GameId, leader, Action.Type.AddToTeam, player);
			}
		}

		internal void AllApproveTeam()
		{
            VoteForTeam(ContextAccess.GamePlay.Game.Players.Count);
		}

		internal void AllDeclineTeam()
		{
			VoteForTeam(0);
		}

		internal void VoteForTeam(int numberOfPlayersForTeam)
		{
            var gameplay = ContextAccess.GamePlay;

            foreach (var player in ContextAccess.GamePlay.Game.Players.Take(numberOfPlayersForTeam))
			{
                gameplay.DoAction(gameplay.Game.GameId, player, Action.Type.VoteApprove);
			}

            foreach (var player in ContextAccess.GamePlay.Game.Players.Skip(numberOfPlayersForTeam))
			{
                gameplay.DoAction(gameplay.Game.GameId, player, Action.Type.VoteReject);
            }
		}

		internal void RemoveMerlin()
		{
            var gameplay = ContextAccess.GamePlay;
            foreach (var merlinPlayer in gameplay.Game.Players.Where(player => player.Character == Character.Merlin))
			{
				merlinPlayer.Character = Character.LoyalServantOfArthur;
			}

            foreach (var assasinPlayer in gameplay.Game.Players.Where(player => player.Character == Character.Assassin))
			{
				assasinPlayer.Character = Character.MinionOfMordred;
			}

            var merlinCount = gameplay.Game.AvailableCharacters.Count(character => character == Character.Merlin);

            gameplay.Game.AvailableCharacters.RemoveAll(character => character == Character.Merlin);

			for (var i = 0; i < merlinCount; i++)
                gameplay.Game.AvailableCharacters.Add(Character.LoyalServantOfArthur);

            var assasinCount = gameplay.Game.AvailableCharacters.Count(character => character == Character.Assassin);

            gameplay.Game.AvailableCharacters.RemoveAll(character => character == Character.Assassin);

			for (var i = 0; i < assasinCount; i++)
                gameplay.Game.AvailableCharacters.Add(Character.MinionOfMordred);
		}

		internal void CompleteQuest(int roundNumber, bool successful)
		{
            var gameplay = ContextAccess.GamePlay;

            //build team
            for (int i = 0; i < gameplay.CurrentQuest.TeamSize; i++)
            {
                gameplay.DoAction(gameplay.Game.GameId, gameplay.CurrentQuest.CurrentVoteTrack.Leader, Action.Type.AddToTeam, gameplay.Game.Players[i]);
            }

            //pass the vote
            foreach (var player in gameplay.Game.Players)
            {
                gameplay.DoAction(gameplay.Game.GameId, player, Action.Type.VoteApprove);
            }

            //do the quest
            foreach (var player in gameplay.CurrentQuest.CurrentVoteTrack.Players)
            {
                gameplay.DoAction(gameplay.Game.GameId, player, successful ? Action.Type.SucceedQuest : Action.Type.FailQuest);
            }
        }

		internal void AddMerlin()
		{
            var gameplay = ContextAccess.GamePlay;
            gameplay.Game.Players.First(p => p.Character == Character.LoyalServantOfArthur).Character = Character.Merlin;
            gameplay.Game.AvailableCharacters[gameplay.Game.AvailableCharacters.IndexOf(Character.LoyalServantOfArthur)] = Character.Merlin;
		}

        internal void AddAssassin()
        {
            var gameplay = ContextAccess.GamePlay;
            gameplay.Game.Players.First(p => p.Character == Character.LoyalServantOfArthur).Character = Character.Assassin;
            gameplay.Game.AvailableCharacters[gameplay.Game.AvailableCharacters.IndexOf(Character.LoyalServantOfArthur)] = Character.Assassin;
        }

		internal void PickMerlin(bool successfullMerlinPick)
		{
            var gameplay = ContextAccess.GamePlay;

            var assasin = gameplay.Game.Players.First(player => player.Character == Character.Assassin);

			if (successfullMerlinPick)
			{
                gameplay.DoAction(gameplay.Game.GameId, assasin, Action.Type.GuessMerlin, gameplay.Game.Players.First(player => player.Character == Character.Merlin));
			}
			else
			{
                gameplay.DoAction(gameplay.Game.GameId, assasin, Action.Type.GuessMerlin, gameplay.Game.Players
                    .Where(player => player.Character == Character.LoyalServantOfArthur || player.Character == Character.Percival)
                    .First(player => player.Character != Character.Merlin));
			}
		}

		void IncrementRound()
		{
            var gameplay = ContextAccess.GamePlay;
            var roundNumber = gameplay.Quests.Count;
            CompleteQuest(roundNumber, roundNumber % 2 == 0);
		}
	}
}