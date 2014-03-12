using System.Linq;
using System;
using TechTalk.SpecFlow;
using System.Collections.Generic;

namespace ResistanceOnline.Core.Test.SpecFlow
{
	internal class GameAssistant
	{
		internal void CreateStandardGame(int numberOfPlayers)
		{
            var owner = Guid.NewGuid();
            var actions = new List<Action>();

            for (var i = 0; i < numberOfPlayers; i++)
            {
                actions.Add(new Action(Guid.NewGuid(), Action.Type.Join, string.Format("player{0}", i)));
                //actions.Add(new Action(owner, Action.Type.AddCharacterCard, Character.LoyalServantOfArthur.ToString()));
            }

            actions.Add(new Action(owner, Action.Type.Start, "0"));

            var game = new Game(actions);
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
			var leader = game.CurrentQuest.CurrentVoteTrack.Leader;
            game.DoAction(leader.Guid, Action.Type.AddToTeam, leader.Name);
            foreach (var player in game.Players.Where(player => player != leader).Take(game.CurrentQuest.TeamSize - 1))
			{
                game.DoAction(leader.Guid, Action.Type.AddToTeam, player.Name);
			}
		}

		internal void AllApproveTeam()
		{
            VoteForTeam(ContextAccess.Game.Players.Count);
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
                game.DoAction(player.Guid, Action.Type.VoteApprove);
			}

            foreach (var player in ContextAccess.Game.Players.Skip(numberOfPlayersForTeam))
			{
                game.DoAction(player.Guid, Action.Type.VoteReject);
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
            for (int i = 0; i < game.CurrentQuest.TeamSize; i++)
            {
                game.DoAction(game.CurrentQuest.CurrentVoteTrack.Leader.Guid, Action.Type.AddToTeam, game.Players[i].Name);
            }

            //pass the vote
            foreach (var player in game.Players)
            {
                game.DoAction(player.Guid, Action.Type.VoteApprove);
            }

            //do the quest
            foreach (var player in game.CurrentQuest.CurrentVoteTrack.Players)
            {
                game.DoAction(player.Guid, successful ? Action.Type.SucceedQuest : Action.Type.FailQuest);
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
                game.DoAction(assasin.Guid, Action.Type.GuessMerlin, game.Players.First(player => player.Character == Character.Merlin).Name);
			}
			else
			{
                game.DoAction(assasin.Guid, Action.Type.GuessMerlin, game.Players
                    .Where(player => player.Character == Character.LoyalServantOfArthur || player.Character == Character.Percival)
                    .First(player => player.Character != Character.Merlin).Name);
			}
		}

		void IncrementRound()
		{
            var game = ContextAccess.Game;
            var roundNumber = game.Quests.Count;
            CompleteQuest(roundNumber, roundNumber % 2 == 0);
		}
	}
}