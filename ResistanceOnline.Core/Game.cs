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

        public List<Character> CharacterCards { get; set; }
        public List<Player> Players { get; set; }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }
        public List<Rule> Rules { get; set; }
        public Player InitialHolderOfLadyOfTheLake { get; set; }
        public Player InitialLeader { get; set; }
        public List<QuestSize> RoundTables;

        public State GameState { get; set; }
        public List<Quest> Quests { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }

        public bool CurrentLancelotAllegianceSwitched { get; set; }
        public Player CurrentHolderOfLadyOfTheLake { get; set; }
        public Quest CurrentQuest { get { return Quests.LastOrDefault(); } }
        public VoteTrack CurrentVoteTrack { get { return CurrentQuest == null ? null : CurrentQuest.CurrentVoteTrack; } }   // todo - use the fancy "?." operator :)   

        public Game(List<Action> actions)
        {
            Players = new List<Player>();
            CharacterCards = new List<Character>();
            LoyaltyDeck = new List<LoyaltyCard> { LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.NoChange, LoyaltyCard.SwitchAlegiance, LoyaltyCard.SwitchAlegiance };
            Rules = new List<Rule>();
            RoundTables = StandardQuestSizes(0);
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
           
            RoundTables = StandardQuestSizes(Players.Count);
        }

        bool ContainsLancelot()
        {
            return (CharacterCards.Contains(Character.EvilLancelot) || CharacterCards.Contains(Character.Lancelot));
        }

        public LoyaltyCard? GetLoyaltyCard(int roundNumber)
        {
            if (Rules.Contains(Rule.LoyaltyCardsAreDeltInAdvance))
                return null;
            if (!ContainsLancelot())
                return null;
            return LoyaltyDeck[roundNumber - 1];
        }

        void AllocateCharacters(int seed)
        {
            var characterCards = CharacterCards.ToList();
            Random random = new Random(seed);
            foreach (var player in Players)
            {
                var index = random.Next(characterCards.Count);
                player.Character = characterCards[index];
                characterCards.RemoveAt(index);
            }
        }

        void ChooseLeader(int seed)
        {
            InitialHolderOfLadyOfTheLake = Players.Random(seed);
            InitialLeader = Players.Next(InitialHolderOfLadyOfTheLake);
        }


        List<QuestSize> StandardQuestSizes(int GameSize)
        {
            var questSizes = new List<QuestSize>();
            if (GameSize <= 5)
            {
                questSizes.Add(new QuestSize(2));
                questSizes.Add(new QuestSize(3));
                questSizes.Add(new QuestSize(2));
                questSizes.Add(new QuestSize(3));
                questSizes.Add(new QuestSize(3));
                return questSizes;
            }

            switch (GameSize)
            {
                case 6:
                    questSizes.Add(new QuestSize(2));
                    questSizes.Add(new QuestSize(3));
                    questSizes.Add(new QuestSize(4));
                    questSizes.Add(new QuestSize(3));
                    questSizes.Add(new QuestSize(4));
                    break;
                case 7:
                    questSizes.Add(new QuestSize(2));
                    questSizes.Add(new QuestSize(3));
                    questSizes.Add(new QuestSize(3));
                    questSizes.Add(new QuestSize(4, 2));
                    questSizes.Add(new QuestSize(4));
                    break;
                case 8:
                    questSizes.Add(new QuestSize(3));
                    questSizes.Add(new QuestSize(4));
                    questSizes.Add(new QuestSize(4));
                    questSizes.Add(new QuestSize(5, 2));
                    questSizes.Add(new QuestSize(5));
                    break;
                case 9:
                    questSizes.Add(new QuestSize(3));
                    questSizes.Add(new QuestSize(4));
                    questSizes.Add(new QuestSize(4));
                    questSizes.Add(new QuestSize(5, 2));
                    questSizes.Add(new QuestSize(5));
                    break;
                default:
                    questSizes.Add(new QuestSize(3));
                    questSizes.Add(new QuestSize(4));
                    questSizes.Add(new QuestSize(4));
                    questSizes.Add(new QuestSize(5, 2));
                    questSizes.Add(new QuestSize(5));
                    break;
            }
            return questSizes;
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

        void StartGame(int seed)
        {
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


        public List<AvailableAction> AvailableActions(Player player)
        {
            var actions = new List<AvailableAction>();
            switch (GameState)
            {
                case State.Lobby:
                    if (player == null && Players.Count < 10)
                    {
                        actions.Add(AvailableAction.FreeText(Action.Type.Join));
                    }
                    actions.Add(AvailableAction.FreeText(Action.Type.AddBot));
                    if (CharacterCards.Count < Players.Count)
                    {
                        actions.Add(AvailableAction.List(Action.Type.AddCharacterCard, Enum.GetNames(typeof(Character)).ToList()));
                    }
                    if (CharacterCards.Count > 0)
                    {
                        actions.Add(AvailableAction.List(Action.Type.RemoveCharacterCard, CharacterCards.Distinct().Select(t=>t.ToString()).ToList()));
                    }
                    if (CharacterCards.Count == Players.Count) //todo check for valid round tables
                    {
                        actions.Add(AvailableAction.FreeText(Action.Type.Start));
                    }
                    break;
                case State.ChoosingTeam:
                    if (CurrentVoteTrack != null && player == CurrentVoteTrack.Leader)
                    {
                        actions.Add(AvailableAction.List(Action.Type.AddToTeam, Players.Except(CurrentVoteTrack.Players).Select(p=>p.Name).ToList()));
                    }
                    break;
                case State.AssigningExcalibur:
                    if (player == CurrentVoteTrack.Leader)
                    {
                        actions.Add(AvailableAction.List(Action.Type.AssignExcalibur, CurrentVoteTrack.Players.Where(p=>p!=CurrentVoteTrack.Leader).Select(p => p.Name).ToList()));
                    }
                    break;
                case State.VotingForTeam:
                    if (!CurrentVoteTrack.Votes.Any(v => v.Player == player))
                    {
                        actions.Add(AvailableAction.ActionOnly(Action.Type.VoteApprove));
                        actions.Add(AvailableAction.ActionOnly(Action.Type.VoteReject));
                    }
                    break;
                case State.Questing:
                    if (CurrentVoteTrack.Players.Contains(player) && !CurrentVoteTrack.QuestCards.Any(q => q.Player == player))
                    {
                        //good must always vote success
                        if (Rules.Contains(Rule.GoodMustAlwaysSucceedQuests) && !IsCharacterEvil(player.Character, CurrentLancelotAllegianceSwitched))
                        {
                            actions.Add(AvailableAction.ActionOnly(Action.Type.SucceedQuest));
                        }
                        else
                        {
                            //lancelot fanatasism
                            if (Rules.Contains(Rule.LancelotsMustVoteFanatically) && (player.Character == Character.Lancelot || player.Character == Character.EvilLancelot))
                            {
                                if (IsCharacterEvil(player.Character, CurrentLancelotAllegianceSwitched))
                                {
                                    actions.Add(AvailableAction.ActionOnly(Action.Type.FailQuest));
                                }
                                else
                                {
                                    actions.Add(AvailableAction.ActionOnly(Action.Type.SucceedQuest));
                                }
                            }
                            else
                            {
                                actions.Add(AvailableAction.ActionOnly(Action.Type.SucceedQuest));
                                actions.Add(AvailableAction.ActionOnly(Action.Type.FailQuest));
                            }
                        }
                    }
                    break;
                case State.UsingExcalibur:
                    if (CurrentVoteTrack.Excalibur.Holder == player)
                    {
                        actions.Add(AvailableAction.List(Action.Type.UseExcalibur, CurrentVoteTrack.Players.Where(p => p != player).Select(p => p.Name).ToList()));
                    }
                    break;
                case State.LadyOfTheLake:
                    if (Rules.Contains(Rule.LadyOfTheLakeExists) && CurrentQuest.LadyOfTheLake != null && CurrentQuest.LadyOfTheLake.Holder == player && CurrentQuest.LadyOfTheLake.Target == null)
                    {
                        var list = Players.Where(p => Quests.Any(r => r.LadyOfTheLake != null && r.LadyOfTheLake.Holder == p)).Select(p => p.Name).ToList();
                        actions.Add(AvailableAction.List(Action.Type.UseTheLadyOfTheLake, list));
                    }
                    break;
                case Game.State.GuessingMerlin:
                    if (player != null && player.Character == Character.Assassin)
                    {
                        actions.Add(AvailableAction.List(Action.Type.GuessMerlin, Players.Where(p => p != player).Select(p => p.Name).ToList()));
                    }
                    break;
            }
            if (player != null)
            {
                actions.Add(AvailableAction.FreeText(Action.Type.Message));
            }
            return actions;
        }

        void DoActions(List<Action> actions)
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
            var availableAction = AvailableActions(owner).FirstOrDefault(a=>a.Action == action.ActionType);
            if (availableAction == null)
            {
                throw new InvalidOperationException("Action not valid");
            }
            if (availableAction.AvailableOptions == AvailableAction.Type.List && !availableAction.Options.Contains(action.Text))
            {
                throw new InvalidOperationException("Action option not valid");
            }

            switch (action.ActionType)
            {
                case Action.Type.Join:
                    JoinGame(action.Text, action.Owner);
                    break;
                case Action.Type.AddCharacterCard:
                    AddCard(action.Text);
                    break;
                case Action.Type.AddBot:
                    JoinGame(action.Text, Guid.NewGuid(), Player.Type.TrustBot);
                    break;
                case Action.Type.Start:
                    StartGame(int.Parse(action.Text));
                    break;
                case Action.Type.Message:
                    Message(owner, action.Text);
                    break;
                case Action.Type.AddToTeam:
                    AddToTeam(target);
                    break;
                case Action.Type.AssignExcalibur:
                    AssignExcalibur(target);
                    break;
                case Action.Type.FailQuest:
                    SubmitQuest(owner, false);
                    break;
                case Action.Type.GuessMerlin:
                    GuessMerlin(target);
                    break;
                case Action.Type.SucceedQuest:
                    SubmitQuest(owner, true);
                    break;
                case Action.Type.UseExcalibur:
                    UseExcalibur(owner, target);
                    break;
                case Action.Type.UseTheLadyOfTheLake:
                    UseLadyOfTheLake(target);
                    break;
                case Action.Type.VoteApprove:
                    VoteForTeam(owner, true);
                    break;
                case Action.Type.VoteReject:
                    VoteForTeam(owner, false);
                    break;
            }
        }

        private void AddCard(string card)
        {
            Character character;
            if (!Enum.TryParse(card, out character))
            {
                throw new ArgumentOutOfRangeException("unknown card " + card);
            }

            CharacterCards.Add(character);
        }

        private void RemoveCard(string card)
        {
            Character character;
            if (!Enum.TryParse(card, out character))
            {
                throw new ArgumentOutOfRangeException("unknown card " + card);
            }

            CharacterCards.Remove(character);
        }

        void GuessMerlin(Player guess)
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
            var quest = new Quest(CurrentHolderOfLadyOfTheLake, roundTable.TeamSize, roundTable.RequiredFails);
            var team = new VoteTrack(leader, roundTable.TeamSize, roundTable.RequiredFails);
            quest.VoteTracks.Add(team);
            Quests.Add(quest);
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
                if (CharacterCards.Contains(Character.Assassin))
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
                CurrentLancelotAllegianceSwitched = !CurrentLancelotAllegianceSwitched;
            }

            NextQuest(Players.Next(CurrentQuest.CurrentVoteTrack.Leader));
        }


        void AddToTeam(Player proposedPlayer)
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

        void AssignExcalibur(Player proposedPlayer)
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
                    CurrentQuest.VoteTracks.Add(new VoteTrack(Players.Next(CurrentVoteTrack.Leader), CurrentQuest.QuestSize, CurrentQuest.RequiredFails));
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

        void UseLadyOfTheLake(Player target)
        {
            if (Quests.Any(r => r.LadyOfTheLake != null && r.LadyOfTheLake.Holder == target))
                throw new Exception("Once a lady has gone " + target + ", she does NOT go back..");

            CurrentQuest.LadyOfTheLake.Target = target;
            CurrentQuest.LadyOfTheLake.IsEvil = IsCharacterEvil(target.Character, CurrentLancelotAllegianceSwitched);
            CurrentHolderOfLadyOfTheLake = target;

            QuestFinished();
        }

    }
}
