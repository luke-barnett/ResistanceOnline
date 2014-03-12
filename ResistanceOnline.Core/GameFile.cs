using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// a state engine to manage all game play
    /// </summary>
    public class Game
    {
        public enum State
        {
            Lobby,
            ChoosingTeam,
            AssigningExcalibur,
            VotingForTeam,
            VoteFailed,
            Questing,
            UsingExcalibur,
            LadyOfTheLake,
            GuessingMerlin,
            EvilTriumphs,
            EternalChaos,
            GoodPrevails,
        }

        public List<Character> AvailableCharacters { get; set; }
        public List<Player> Players { get; set; }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }
        public List<Rule> Rules { get; set; }
        public Player InitialHolderOfLadyOfTheLake { get; set; }
        public Player InitialLeader { get; set; }
        public List<QuestSize> RoundTables;


        public State GameState { get; set; }
        public List<Quest> Quests { get; set; }
        public Player HolderOfLadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }
        public Quest CurrentQuest { get { return Quests.LastOrDefault(); } }
        public VoteTrack CurrentVoteTrack { get { return CurrentQuest == null ? null : CurrentQuest.CurrentVoteTrack; } }   // todo - use the fancy "?." operator :)   

        public Game(List<Action> actions)
        {
            Players = new List<Player>();
            AvailableCharacters = new List<Character>();
            LoyaltyDeck = new List<LoyaltyCard> { LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.SwitchAlegiance, LoyaltyCard.SwitchAlegiance };
            Rules = new List<Rule>() { Rule.LadyOfTheLakeExists };
            RoundTables = StandardRoundTables(0);
            Quests = new List<Quest>();

            DoActions(actions);
        }

        void JoinGame(string playerName, Guid playerGuid, Player.Type playerType = Player.Type.Human)
        {
            if (Players.Select(p => p.Guid).Contains(playerGuid))
                throw new InvalidOperationException("It's really not fair if you play as more than one player and you want the game to be fair don't you?");

            if (String.IsNullOrWhiteSpace(playerName))
                playerName = String.Empty;

            playerName = playerName.Uniquify(Players.Select(p => p.Name));

            Players.Add(new Player() { Name = playerName, Guid = playerGuid, PlayerType = playerType });

            var evilCount = AvailableCharacters.Count(c => IsCharacterEvil(c, false));
            if (evilCount < (AvailableCharacters.Count / 3.0))
            {
                AvailableCharacters.Add(Character.MinionOfMordred);
            }
            else
            {
                AvailableCharacters.Add(Character.LoyalServantOfArthur);
            }

            RoundTables = StandardRoundTables(Players.Count);
        }

        public bool ContainsLancelot()
        {
            return (AvailableCharacters.Contains(Character.EvilLancelot) || AvailableCharacters.Contains(Character.Lancelot));
        }

        public LoyaltyCard? GetLoyaltyCard(int roundNumber)
        {
            if (Rules.Contains(Rule.LoyaltyCardsAreDeltInAdvance))
                return null;
            if (!ContainsLancelot())
                return null;
            return LoyaltyDeck[roundNumber - 1];
        }

        public void AllocateCharacters(int seed)
        {
            var characterCards = AvailableCharacters.ToList();
            Random random = new Random(seed);
            foreach (var player in Players)
            {
                var index = random.Next(characterCards.Count);
                player.Character = characterCards[index];
                characterCards.RemoveAt(index);
            }
        }

        public void ChooseLeader(int seed)
        {
            InitialHolderOfLadyOfTheLake = Players.Random(seed);
            InitialLeader = Players.Next(InitialHolderOfLadyOfTheLake);
        }


        public List<QuestSize> StandardRoundTables(int GameSize)
        {
            var roundTables = new List<QuestSize>();
            if (GameSize <= 5)
            {
                roundTables.Add(new QuestSize(2));
                roundTables.Add(new QuestSize(3));
                roundTables.Add(new QuestSize(2));
                roundTables.Add(new QuestSize(3));
                roundTables.Add(new QuestSize(3));
                return roundTables;
            }

            switch (GameSize)
            {
                case 6:
                    roundTables.Add(new QuestSize(2));
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    break;
                case 7:
                    roundTables.Add(new QuestSize(2));
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4, 2));
                    roundTables.Add(new QuestSize(4));
                    break;
                case 8:
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(5, 2));
                    roundTables.Add(new QuestSize(5));
                    break;
                case 9:
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(5, 2));
                    roundTables.Add(new QuestSize(5));
                    break;
                default:
                    roundTables.Add(new QuestSize(3));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(4));
                    roundTables.Add(new QuestSize(5, 2));
                    roundTables.Add(new QuestSize(5));
                    break;
            }
            return roundTables;
        }

        public bool IsCharacterEvil(Character character, bool lancelotAllegianceSwitched)
        {
            switch (character)
            {
                case Core.Character.Assassin:
                case Core.Character.MinionOfMordred:
                case Core.Character.Mordred:
                case Core.Character.Morgana:
                case Core.Character.Oberon:
                    return true;
                case Core.Character.Lancelot:
                    if (lancelotAllegianceSwitched)
                    {
                        return true;
                    }
                    break;
                case Core.Character.EvilLancelot:
                    if (!lancelotAllegianceSwitched)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        void StartGame(string seedText)
        {
            int seed = int.Parse(seedText);
            AllocateCharacters(seed);
            ChooseLeader(seed);
            LoyaltyDeck = LoyaltyDeck.Shuffle(seed).ToList();
            NextQuest(InitialLeader);
        }

        void Message(Player player, string message)
        {
            CurrentQuest.CurrentVoteTrack.Messages.Add(new PlayerMessage { Player = player, Message = message });
        }

        public Knowledge PlayerKnowledge(Player myself, Player someoneelse)
        {
            if (myself == null)
                return Knowledge.Player;

            //lancelots can know each other
            if (Rules.Contains(Rule.LancelotsKnowEachOther))
            {
                if ((myself.Character == Character.Lancelot || myself.Character == Character.EvilLancelot) && (someoneelse.Character == Character.Lancelot))
                {
                    return Knowledge.Lancelot;
                }
                if ((myself.Character == Character.Lancelot || myself.Character == Character.EvilLancelot) && (someoneelse.Character == Character.EvilLancelot))
                {
                    return Knowledge.EvilLancelot;
                }
            }

            //minions know each other (except oberon)
            if (myself.Character == Character.Assassin || myself.Character == Character.Morgana || myself.Character == Character.MinionOfMordred || myself.Character == Character.Mordred)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morgana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Mordred || someoneelse.Character == Character.EvilLancelot)
                {
                    return Knowledge.Evil;
                }
            }

            //merlin knows minions (except mordred)
            if (myself.Character == Character.Merlin)
            {
                if (someoneelse.Character == Character.Assassin || someoneelse.Character == Character.Morgana || someoneelse.Character == Character.MinionOfMordred || someoneelse.Character == Character.Oberon)
                {
                    return Knowledge.Evil;
                }
            }

            //percy knows merlin and morgana
            if (myself.Character == Character.Percival)
            {
                if (someoneelse.Character == Character.Merlin || someoneelse.Character == Character.Morgana)
                {
                    return Knowledge.Magical;
                }
            }

            return Knowledge.Player;
        }


        public List<Action.Type> AvailableActions(Player player)
        {
            var actions = new List<Action.Type>();
            switch (GameState)
            {
                case State.Lobby:
                    if (player == null && Players.Count < 10)
                    {
                        actions.Add(Action.Type.Join);
                    }
                    //todo - prevent game start unless valid
                    actions.Add(Action.Type.Start);
                    break;
                case State.ChoosingTeam:
                    if (CurrentVoteTrack != null && player == CurrentVoteTrack.Leader)
                    {
                        actions.Add(Action.Type.AddToTeam);
                    }
                    break;
                case State.AssigningExcalibur:
                    if (player == CurrentVoteTrack.Leader)
                    {
                        actions.Add(Action.Type.AssignExcalibur);
                    }
                    break;
                case State.VotingForTeam:
                    if (!CurrentVoteTrack.Votes.Any(v => v.Player == player))
                    {
                        actions.Add(Action.Type.VoteApprove);
                        actions.Add(Action.Type.VoteReject);
                    }
                    break;
                case State.Questing:
                    if (CurrentVoteTrack.Players.Contains(player) && !CurrentVoteTrack.QuestCards.Any(q => q.Player == player))
                    {
                        //good must always vote success
                        if (Rules.Contains(Rule.GoodMustAlwaysSucceedQuests) && !IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
                        {
                            actions.Add(Action.Type.SucceedQuest);
                        }
                        else
                        {
                            //lancelot fanatasism
                            if (Rules.Contains(Rule.LancelotsMustVoteFanatically) && (player.Character == Character.Lancelot || player.Character == Character.EvilLancelot))
                            {
                                if (IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
                                {
                                    actions.Add(Action.Type.FailQuest);
                                }
                                else
                                {
                                    actions.Add(Action.Type.SucceedQuest);
                                }
                            }
                            else
                            {
                                actions.Add(Action.Type.SucceedQuest);
                                actions.Add(Action.Type.FailQuest);
                            }
                        }
                    }
                    break;
                case State.UsingExcalibur:
                    if (CurrentVoteTrack.Excalibur.Holder == player)
                    {
                        actions.Add(Action.Type.UseExcalibur);
                    }
                    break;
                case State.LadyOfTheLake:
                    if (Rules.Contains(Rule.LadyOfTheLakeExists) && CurrentQuest.LadyOfTheLake != null && CurrentQuest.LadyOfTheLake.Holder == player && CurrentQuest.LadyOfTheLake.Target == null)
                    {
                        actions.Add(Action.Type.UseTheLadyOfTheLake);
                    }
                    break;
                case Game.State.GuessingMerlin:
                    if (player != null && player.Character == Character.Assassin)
                        actions.Add(Action.Type.GuessMerlin);
                    break;
            }
            actions.Add(Action.Type.Message);
            return actions;
        }

        public void DoActions(List<Action> actions)
        {
            foreach (var action in actions)
            {
                DoAction(action);
            }
        }

        public void DoAction(Guid owner, Action.Type actionType, string targetPlayerName)
        {
            DoAction(new Action(owner, actionType, targetPlayerName));
        }

        public void DoAction(Guid owner, Action.Type actionType)
        {
            DoAction(new Action(owner, actionType));
        }

        public void DoAction(Action action)
        {
            var owner = Players.FirstOrDefault(p => p.Guid == action.Owner);
            var target = Players.FirstOrDefault(p => p.Name == action.Text);
            if (!AvailableActions(owner).Contains(action.ActionType))
            {
                throw new InvalidOperationException("Action not valid");
            }

            switch (action.ActionType)
            {
                case Action.Type.Join:
                    JoinGame(action.Text, action.Owner);
                    break;
                case Action.Type.Start:
                    StartGame(action.Text);
                    break;
                case Action.Type.Message:
                    Message(owner, action.Text);
                    break;
                case Action.Type.AddToTeam:
                    AddToTeam(owner, target);
                    break;
                case Action.Type.AssignExcalibur:
                    AssignExcalibur(owner, target);
                    break;
                case Action.Type.FailQuest:
                    SubmitQuest(owner, false);
                    break;
                case Action.Type.GuessMerlin:
                    GuessMerlin(owner, target);
                    break;
                case Action.Type.SucceedQuest:
                    SubmitQuest(owner, true);
                    break;
                case Action.Type.UseExcalibur:
                    UseExcalibur(owner, target);
                    break;
                case Action.Type.UseTheLadyOfTheLake:
                    UseLadyOfTheLake(owner, target);
                    break;
                case Action.Type.VoteApprove:
                    VoteForTeam(owner, true);
                    break;
                case Action.Type.VoteReject:
                    VoteForTeam(owner, false);
                    break;
            }
        }

        void GuessMerlin(Player player, Player guess)
        {
            AssassinsGuessAtMerlin = guess;

            if (guess.Character == Character.Merlin)
            {
                GameState = State.EvilTriumphs;
            }
            else
            {
                GameState = State.GoodPrevails;
            }
        }

        void NextQuest(Player leader)
        {
            if (RoundTables.Count == 0)
                return;

            var roundTable = RoundTables[Quests.Count];
            var round = new Quest(Players, leader, HolderOfLadyOfTheLake, roundTable.TeamSize, roundTable.RequiredFails, LancelotAllegianceSwitched);
            var team = new VoteTrack(leader, roundTable.TeamSize, roundTable.RequiredFails);
            round.VoteTracks.Add(team);
            Quests.Add(round);
            GameState = State.ChoosingTeam;
        }

        void QuestFinished()
        {
            //3 failed missions, don't bother going any further
            if (Quests.Count(r => !r.IsSuccess.Value) >= 3)
            {
                GameState = State.EvilTriumphs;
                return;
            }

            //3 successful missions, don't bother going any further
            if (Quests.Count(r => r.IsSuccess.Value) >= 3)
            {
                if (AvailableCharacters.Contains(Character.Assassin))
                {
                    GameState = State.GuessingMerlin;
                }
                else
                {
                    GameState = State.GoodPrevails;
                }
                return;
            }

            //loyalty cards
            if (GetLoyaltyCard(Quests.Count) == LoyaltyCard.SwitchAlegiance)
            {
                LancelotAllegianceSwitched = !LancelotAllegianceSwitched;
            }

            NextQuest(Players.Next(CurrentQuest.CurrentVoteTrack.Leader));
        }


        void AddToTeam(Player player, Player proposedPlayer)
        {
            if (CurrentVoteTrack.Players.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is already on the team");

            CurrentVoteTrack.Players.Add(proposedPlayer);

            if (CurrentVoteTrack.Players.Count == CurrentVoteTrack.QuestSize)
            {
                if (Rules != null && Rules.Contains(Rule.ExcaliburExists))
                {
                    GameState = State.AssigningExcalibur;
                }
                else
                {
                    GameState = State.VotingForTeam;
                }
            }
        }

        void AssignExcalibur(Player player, Player proposedPlayer)
        {
            if (!CurrentVoteTrack.Players.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is not on team..");

            if (proposedPlayer == CurrentVoteTrack.Leader)
                throw new InvalidOperationException("Leader cannot assign excalibur to themself");

            CurrentVoteTrack.Excalibur.Holder = proposedPlayer;

            GameState = State.VotingForTeam;
        }

        void UseExcalibur(Player player, Player proposedPlayer)
        {
            if (proposedPlayer != null && !CurrentVoteTrack.Players.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is not on team..");

            if (proposedPlayer == player)
                throw new InvalidOperationException("Really? On yourself? How would that work? It's a sword..");

            CurrentVoteTrack.Excalibur.UsedOn = CurrentVoteTrack.QuestCards.First(p => p.Player == proposedPlayer);
            CurrentVoteTrack.Excalibur.OriginalMissionWasSuccess = CurrentVoteTrack.Excalibur.UsedOn.Success;
            CurrentVoteTrack.Excalibur.UsedOn.Success = !CurrentVoteTrack.Excalibur.UsedOn.Success;

            TeamFinished();
        }

        void TeamFinished()
        {
            if (Rules.Contains(Rule.LadyOfTheLakeExists) && Quests.Count >= 2)
            {
                GameState = State.LadyOfTheLake;
                return;
            }

            QuestFinished();
        }

        void VoteForTeam(Player player, bool approve)
        {
            CurrentVoteTrack.Votes.Add(new VoteToken { Approve = approve, Player = player });

            if (CurrentVoteTrack.Votes.Count < Players.Count)
                return;

            //on the last vote, if it fails, create the next quest
            var rejects = CurrentVoteTrack.Votes.Count(v => !v.Approve);
            if (rejects >= Math.Ceiling(Players.Count / 2.0))
            {
                if (CurrentQuest.VoteTracks.Count == 5)
                {
                    GameState = State.EternalChaos;
                }
                else
                {
                    CurrentQuest.VoteTracks.Add(new VoteTrack(Players.Next(CurrentVoteTrack.Leader), CurrentQuest.TeamSize, CurrentQuest.RequiredFails));
                    GameState = State.ChoosingTeam;
                }
            }
            else
            {
                GameState = State.Questing;
            }
        }

        void SubmitQuest(Player player, bool success)
        {
            CurrentVoteTrack.QuestCards.Add(new QuestCard { Player = player, Success = success });

            if (CurrentVoteTrack.QuestCards.Count == CurrentVoteTrack.QuestSize)
            {
                TeamFinished();
            }
        }

        void UseLadyOfTheLake(Player player, Player target)
        {
            if (Quests.Any(r => r.LadyOfTheLake != null && r.LadyOfTheLake.Holder == target))
                throw new Exception("Once a lady has gone " + target + ", she does NOT go back..");

            CurrentQuest.LadyOfTheLake.Target = target;
            CurrentQuest.LadyOfTheLake.IsEvil = IsCharacterEvil(target.Character, LancelotAllegianceSwitched);
            HolderOfLadyOfTheLake = target;

            QuestFinished();
        }

    }
}
