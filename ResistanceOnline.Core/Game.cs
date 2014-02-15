using System;
using System.Collections.Generic;
using System.Linq;

namespace ResistanceOnline.Core
{
	public class Game
	{
		public enum State
		{
			GameSetup,
			Playing,
			EvilTriumphs,
			GoodPrevails,
			GuessingMerlin,
			MerlinDies
		}

		public Game(int players, bool impersonationEnabled)
		{
			ImpersonationEnabled = impersonationEnabled;
			Players = new List<Player>();
			Rounds = new List<Round>();
			AvailableCharacters = new List<Character>();
			GameSize = players;

			RoundTables = new List<RoundTable>();
			switch (players)
			{
				case 5:
					RoundTables.Add(new RoundTable(2));
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(2));
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(3));
					break;

				case 6:
					RoundTables.Add(new RoundTable(2));
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(4));
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(4));
					break;

				case 7:
					RoundTables.Add(new RoundTable(2));
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(4, 2));
					RoundTables.Add(new RoundTable(4));
					break;

				case 8:
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(4));
					RoundTables.Add(new RoundTable(4));
					RoundTables.Add(new RoundTable(5, 2));
					RoundTables.Add(new RoundTable(5));
					break;

				case 9:
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(4));
					RoundTables.Add(new RoundTable(4));
					RoundTables.Add(new RoundTable(5, 2));
					RoundTables.Add(new RoundTable(5));
					break;

				case 10:
					RoundTables.Add(new RoundTable(3));
					RoundTables.Add(new RoundTable(4));
					RoundTables.Add(new RoundTable(4));
					RoundTables.Add(new RoundTable(5, 2));
					RoundTables.Add(new RoundTable(5));
					break;

				default:
					throw new Exception("No tableaus for games with " + players + " players");
			}
		}

		public List<Character> AvailableCharacters { get; set; }

		public int GameSize { get; set; }

		public bool ImpersonationEnabled { get; set; }

		public List<Player> Players { get; set; }

		public List<Round> Rounds { get; set; }

		public int QuestIndicator { get; set; }

		public Player AssassinsGuessAtMerlin { get; set; }

		public List<RoundTable> RoundTables { get; set; }

		public void GuessMerlin(Player player, Player guess)
		{
			if (player.Character != Character.Assassin)
				throw new Exception("Hax. Player is not assassin.");

			if (AssassinsGuessAtMerlin != null)
				throw new Exception("Hax. Assassin has already guessed.");

			AssassinsGuessAtMerlin = guess;
		}

		public void AddCharacter(Character character)
		{
			if (AvailableCharacters.Count == GameSize)
				throw new Exception("All roles added");
			AvailableCharacters.Add(character);

			//on last character, allocate characters
			if (AvailableCharacters.Count == GameSize && Players.Count == GameSize)
			{
				Allocate();
			}
		}

		public void JoinGame(string playerName, Guid playerGuid)
		{
			if (Players.Count == GameSize)
				throw new Exception("Game already full");

			if (Players.Select(p => p.Name).Contains(playerName))
				throw new Exception("Player name already taken");

			Players.Add(new Player() { Name = playerName, Guid = playerGuid });

			//on last player, allocate characters if
			if (AvailableCharacters.Count == GameSize)
			{
				Allocate();
			}
		}

		private void Allocate()
		{
			//on last player, allocate characters
			if (Players.Count == GameSize)
			{
				if (AvailableCharacters.Count != GameSize)
					throw new Exception("Not Enough Characters for Players");

				var characterCards = AvailableCharacters.ToList();
				Random random = new Random();
				foreach (var player in Players)
				{
					var index = random.Next(characterCards.Count);
					player.Character = characterCards[index];
					characterCards.RemoveAt(index);
				}

				//create first round
				CreateRound(random.Next(GameSize));
			}
		}

		private void CreateRound(int leader)
		{
			if (Rounds.Count > RoundTables.Count)
				throw new Exception("round overrun");

			var tableaus = RoundTables[Rounds.Count];
			Rounds.Add(new Round(Players, leader, tableaus.TeamSize, tableaus.RequiredFails));
		}

		public Round CurrentRound { get { return Rounds.Last(); } }

		public void AddToTeam(Player player, Player proposedPlayer)
		{
			CurrentRound.AddToTeam(player, proposedPlayer);
		}

		public void VoteForTeam(Player player, bool approve)
		{
			CurrentRound.VoteForTeam(player, approve);
		}

		public void SubmitQuest(Player player, bool success)
		{
			CurrentRound.SubmitQuest(player, success);

			//on last quest submit, create the next round
			var roundState = CurrentRound.DetermineState();
			if (roundState == Round.State.Succeeded || roundState == Round.State.Failed)
			{
				//3 failed missions, don't bother going any further
				if (Rounds.Where(r => r.DetermineState() == Round.State.Failed).Count() >= 3)
					return;

				//3 successful missions, don't bother going any further
				if (Rounds.Where(r => r.DetermineState() == Round.State.Succeeded).Count() >= 3)
					return;

				//create the next round
				CreateRound(CurrentRound.NextPlayer);
			}
		}

		public State DetermineState()
		{
			if (AvailableCharacters.Count < GameSize || Players.Count < GameSize)
				return State.GameSetup;

			if (Rounds.Where(r => r.DetermineState() == Round.State.Failed).Count() >= 3)
				return State.EvilTriumphs;

			if (Rounds.Where(r => r.DetermineState() == Round.State.Succeeded).Count() >= 3)
			{
				if (AssassinsGuessAtMerlin == null)
					return State.GuessingMerlin;

				if (AssassinsGuessAtMerlin.Character == Character.Merlin)
					return State.MerlinDies;

				return State.GoodPrevails;
			}

			return State.Playing;
		}

		/// <summary>
		/// this is the available actions a player has given who they are and the state of the game
		/// this should show a list of buttons on the webpage or something
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public List<Action.Type> AvailableActions(Player player)
		{
			var gameState = DetermineState();
			switch (gameState)
			{
				case Game.State.Playing:
					var roundState = CurrentRound.DetermineState();
					var quest = CurrentRound.CurrentTeam;
					switch (roundState)
					{
						case Round.State.ProposingPlayers:
							if (player != null && quest.Leader.Name == player.Name)
							{
								return new List<Action.Type>() { Action.Type.AddToTeam };
							}
							return new List<Action.Type>();

						case Round.State.Voting:
							if (player != null && !quest.Votes.Select(v => v.Player.Name).ToList().Contains(player.Name))
							{
								return new List<Action.Type>() { Action.Type.VoteForTeam };
							}
							return new List<Action.Type>();

						case Round.State.Questing:
							if (player != null && quest.TeamMembers.Select(v => v.Name).ToList().Contains(player.Name) &&
								!quest.Quests.Select(q => q.Player.Name).ToList().Contains(player.Name))
							{
								return new List<Action.Type>() { Action.Type.SubmitQuestCard };
							}
							return new List<Action.Type>();
					}

					return new List<Action.Type>();

				case Game.State.GameSetup:
					var actions = new List<Action.Type>();
					if (player == null && Players.Count < GameSize)
					{
						actions.Add(Action.Type.JoinGame);
					}
					if (player != null && AvailableCharacters.Count < GameSize)
					{
						actions.Add(Action.Type.AddCharacter);
					}
					return actions;

				case Game.State.GuessingMerlin:
					if (player != null && player.Character == Character.Assassin)
						return new List<Action.Type>() { Action.Type.GuessMerlin };
					return new List<Action.Type>();

				case Game.State.EvilTriumphs:
				case Game.State.GoodPrevails:
				case Game.State.MerlinDies:
					return new List<Action.Type>();
			}
			return new List<Action.Type>();
		}

		/// <summary>
		/// once a player performs an action, this should update the game state appropriately
		/// </summary>
		/// <param name="player"></param>
		/// <param name="action"></param>
		public void PerformAction(Player player, Action action)
		{
			if (!AvailableActions(player).Contains(action.ActionType))
				throw new Exception(String.Format("Hax. Player {0} can't perform action {1}", player.Name, action));

			switch (action.ActionType)
			{
				case Action.Type.AddCharacter:
					AddCharacter(action.Character);
					break;

				case Action.Type.GuessMerlin:
					GuessMerlin(player, action.Player);
					break;

				case Action.Type.JoinGame:
					JoinGame(action.Name, Guid.NewGuid());
					break;

				case Action.Type.AddToTeam:
					AddToTeam(player, action.Player);
					break;

				case Action.Type.SubmitQuestCard:
					SubmitQuest(player, action.Success);
					break;

				case Action.Type.VoteForTeam:
					VoteForTeam(player, action.Accept);
					break;

				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// does myself know someoneelse is evil. e.g. they are both minions, or myself is merlin
		/// </summary>
		/// <param name="playerSelf"></param>
		/// <param name="playerTarget"></param>
		/// <returns></returns>
		public static bool DetectEvil(Player myself, Player someoneelse)
		{
			if (myself == null)
				return false;

			//minions know each other (except oberon)
			if (myself.Character == Character.Assassin || myself.Character == Character.Morgana || myself.Character == Character.MinionOfMordred || myself.Character == Character.Mordred)
			{
				if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morgana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Mordred)
				{
					return true;
				}
			}

			//merlin knows minions (except mordred)
			if (myself.Character == Character.Merlin)
			{
				if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morgana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Oberon)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// does myself know someoneelse is merlin (or morcana), e.g. myself is percival
		/// </summary>
		/// <param name="playerSelf"></param>
		/// <param name="playerTarget"></param>
		/// <returns></returns>
		public static bool DetectMerlin(Player myself, Player someoneelse)
		{
			if (myself == null)
				return false;

			if (myself.Character == Character.Percival)
			{
				if (someoneelse.Character == Character.Merlin || someoneelse.Character == Character.Morgana)
				{
					return true;
				}
			}

			return false;
		}

		public int GameId { get; set; }
	}
}