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
            Error
        }

        public List<Character> CharacterCards { get; set; }
        public List<Player> Players { get; set; }
        public List<LoyaltyCard> LoyaltyDeck { get; set; }
        public List<Rule> Rules { get; set; }
        public Player InitialHolderOfLadyOfTheLake { get; set; }
        public Player InitialLeader { get; set; }
        public List<QuestSize> RoundTables;
        public List<Player> Winners { get; set; }

        public string GameName { get; set; }

        public State GameState { get; set; }
        public List<Quest> Quests { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }

        public DateTimeOffset LastActionTime { get; set; }

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
            LastActionTime = DateTimeOffset.MinValue;
            Winners = new List<Player>();
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
            Random random = new Random(seed);

            //allocate characters
            var characterCards = CharacterCards.ToList();
            foreach (var player in Players)
            {
                var index = random.Next(characterCards.Count);
                player.Character = characterCards[index];
                characterCards.RemoveAt(index);
            }

            //spin the crown
            InitialHolderOfLadyOfTheLake = Players.Random(seed);
            CurrentHolderOfLadyOfTheLake = InitialHolderOfLadyOfTheLake;
            InitialLeader = Players.Next(InitialHolderOfLadyOfTheLake);

            //shuffle the loyalty deck
            LoyaltyDeck = LoyaltyDeck.Shuffle(seed).ToList();

            //start the game
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
                    else
                    {
                        if (player == Players.First())
                        {
                            actions.Add(AvailableAction.FreeText(Action.Type.SetGameName));
                            actions.Add(AvailableAction.FreeText(Action.Type.AddBot));
                            actions.Add(AvailableAction.Items(Action.Type.AddCharacterCard, Enum.GetNames(typeof(Character)).ToList()));

                            if (CharacterCards.Count > 0)
                            {
                                actions.Add(AvailableAction.Items(Action.Type.RemoveCharacterCard, CharacterCards.Select(t => t.ToString()).ToList()));
                            }

                            var otherRules = Enum.GetValues(typeof(Rule)).Cast<Rule>().ToList().Except(Rules).Select(r=>r.ToString()).ToList();
                            if (otherRules.Count>0) 
                            {
                                actions.Add(AvailableAction.Items(Action.Type.AddRule, otherRules));
                            }
                            if (Rules.Count > 0)
                            {
                                actions.Add(AvailableAction.Items(Action.Type.RemoveRule, Rules.Select(r=>r.ToString()).ToList()));
                            }

                            if (IsValid())
                            {
                                actions.Add(AvailableAction.FreeText(Action.Type.Start));
                            }
                        }
                    }

                    return actions;
                case State.ChoosingTeam:
                    if (CurrentVoteTrack != null && player == CurrentVoteTrack.Leader)
                    {
                        actions.Add(AvailableAction.Items(Action.Type.AddToTeam, Players.Except(CurrentVoteTrack.Players).Select(p=>p.Name).ToList()));
                    }
                    if (CurrentVoteTrack != null && player == CurrentVoteTrack.Leader && CurrentVoteTrack.Players.Any())
                    {
                        actions.Add(AvailableAction.Items(Action.Type.RemoveFromTeam, CurrentVoteTrack.Players.Select(p => p.Name).ToList()));
                    }
                    break;
                case State.AssigningExcalibur:
                    if (player == CurrentVoteTrack.Leader)
                    {
                        actions.Add(AvailableAction.Items(Action.Type.AssignExcalibur, CurrentVoteTrack.Players.Where(p=>p!=CurrentVoteTrack.Leader).Select(p => p.Name).ToList()));
                    }
                    break;
                case State.VotingForTeam:
                    if (player!=null && !CurrentVoteTrack.Votes.Any(v => v.Player == player))
                    {
                        actions.Add(AvailableAction.Action(Action.Type.VoteApprove));
                        actions.Add(AvailableAction.Action(Action.Type.VoteReject));
                    }
                    break;
                case State.Questing:
                    if (CurrentVoteTrack.Players.Contains(player) && !CurrentVoteTrack.QuestCards.Any(q => q.Player == player))
                    {
                        //good must always vote success
                        if (Rules.Contains(Rule.GoodMustAlwaysSucceedQuests) && !IsCharacterEvil(player.Character, CurrentLancelotAllegianceSwitched))
                        {
                            actions.Add(AvailableAction.Action(Action.Type.SucceedQuest));
                        }
                        else
                        {
                            //lancelot fanatasism
                            if (Rules.Contains(Rule.LancelotsMustVoteFanatically) && (player.Character == Character.Lancelot || player.Character == Character.EvilLancelot))
                            {
                                if (IsCharacterEvil(player.Character, CurrentLancelotAllegianceSwitched))
                                {
                                    actions.Add(AvailableAction.Action(Action.Type.FailQuest));
                                }
                                else
                                {
                                    actions.Add(AvailableAction.Action(Action.Type.SucceedQuest));
                                }
                            }
                            else
                            {
                                actions.Add(AvailableAction.Action(Action.Type.SucceedQuest));
                                actions.Add(AvailableAction.Action(Action.Type.FailQuest));
                            }
                        }
                    }
                    break;
                case State.UsingExcalibur:
                    if (CurrentVoteTrack.Excalibur.Holder == player)
                    {
                        actions.Add(AvailableAction.Items(Action.Type.UseExcalibur, CurrentVoteTrack.Players.Where(p => p != player).Select(p => p.Name).ToList()));
                        actions.Last().ActionItems.Insert(0, "");
                    }
                    break;
                case State.LadyOfTheLake:
                    if (Rules.Contains(Rule.LadyOfTheLakeExists) && CurrentQuest.LadyOfTheLake != null && CurrentQuest.LadyOfTheLake.Holder == player && CurrentQuest.LadyOfTheLake.Target == null)
                    {
                        var list = Players.Except(Players.Where(p => Quests.Any(r => r.LadyOfTheLake != null && r.LadyOfTheLake.Holder == p))).Select(p => p.Name).ToList();
                        actions.Add(AvailableAction.Items(Action.Type.UseTheLadyOfTheLake, list));
                    }
                    break;
                case Game.State.GuessingMerlin:
                    if (player != null && player.Character == Character.Assassin)
                    {
                        actions.Add(AvailableAction.Items(Action.Type.GuessMerlin, Players.Where(p => p != player).Select(p => p.Name).ToList()));
                    }
                    break;
            }
            if (player != null)
            {
                actions.Add(AvailableAction.FreeText(Action.Type.Message));
            }
            return actions;
        }

        private bool IsValid()
        {
            //not enough cards
            if (CharacterCards.Count != Players.Count)
                return false;

            //not enough players
            if (RoundTables.Select(r => r.TeamSize).Max() > Players.Count)
                return false;

            //invalid rounds - more fails required than people on team
            if (RoundTables.Any(r => r.RequiredFails > r.TeamSize))
                return false;
            
            var evilCount = Players.Count(p=>IsCharacterEvil(p.Character, false));

            //impossible to win rounds
            if (RoundTables.Any(r => r.TeamSize - r.RequiredFails > evilCount))
                return false;

            //impossible to loose rounds
            if (RoundTables.Any(r => r.RequiredFails > evilCount))
                return false;

            return true;
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
            if (action.ActionType == Action.Type.AddBot)
            {
                owner = Players.First();
            }

            if (owner == null && action.ActionType != Action.Type.Join)
            {
                throw new InvalidOperationException(String.Format("player guid {0} could not be mapped to a valid player", action.Owner));
            }
            var target = Players.FirstOrDefault(p => p.Name == action.Text);
            var availableAction = AvailableActions(owner).FirstOrDefault(a=>a.ActionType == action.ActionType);
            if (availableAction == null)
            {
                throw new InvalidOperationException(String.Format("Action {0} not valid for {1} while in state {2}", action.ActionType, owner.Name, GameState));
            }
            if (availableAction.AvailableActionType == AvailableAction.Type.Items && !availableAction.ActionItems.Contains(action.Text))
            {
                throw new InvalidOperationException(String.Format("{2} is not a valid choice for {0} by {1}", action.ActionType, owner.Name, action.Text));
            }

            LastActionTime = action.Timestamp;

            switch (action.ActionType)
            {
                case Action.Type.Join:
                    JoinGame(action.Text, action.Owner);
                    break;
                case Action.Type.SetGameName:
                    GameName = action.Text;
                    break;
                case Action.Type.AddCharacterCard:
                    AddCard(action.Text);
                    break;
                case Action.Type.RemoveCharacterCard:
                    RemoveCard(action.Text);
                    break;
                case Action.Type.AddRule:
                    AddRule(action.Text);
                    break;
                case Action.Type.RemoveRule:
                    RemoveRule(action.Text);
                    break;
                case Action.Type.AddBot:
                    JoinGame(action.Text, action.Owner, Player.Type.TrustBot);
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
                case Action.Type.RemoveFromTeam:
                    RemoveFromTeam(target);
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

        private void RemoveFromTeam(Player target)
        {
            CurrentVoteTrack.Players.Remove(target);
        }

        private void AddRule(string rule)
        {
            Rules.Add((Rule)Enum.Parse(typeof(Rule), rule));
        }

        private void RemoveRule(string rule)
        {
            Rules.Remove((Rule)Enum.Parse(typeof(Rule), rule));
        }

        private void AddCard(string card)
        {
            CharacterCards.Add((Character)Enum.Parse(typeof(Character), card));
        }

        private void RemoveCard(string card)
        {
            CharacterCards.Remove((Character)Enum.Parse(typeof(Character), card));
        }

        void GuessMerlin(Player guess)
        {
            AssassinsGuessAtMerlin = guess;

            if (guess.Character == Character.Merlin)
            {
                GameState = State.EvilTriumphs;
                Winners = Players.Where(p => IsCharacterEvil(p.Character, CurrentLancelotAllegianceSwitched)).ToList();
            }
            else
            {
                Winners = Players.Where(p => !IsCharacterEvil(p.Character, CurrentLancelotAllegianceSwitched)).ToList();
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
                Winners = Players.Where(p => IsCharacterEvil(p.Character, CurrentLancelotAllegianceSwitched)).ToList();
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
                    Winners = Players.Where(p => !IsCharacterEvil(p.Character, CurrentLancelotAllegianceSwitched)).ToList();
                }
                return;
            }

            //lady of the lake
            if (Rules.Contains(Rule.LadyOfTheLakeExists) && Quests.Count >= 2 && (CurrentQuest.LadyOfTheLake == null || CurrentQuest.LadyOfTheLake.Target == null))
            {
                GameState = State.LadyOfTheLake;
                CurrentQuest.LadyOfTheLake = new LadyOfTheLakeUse { Holder = CurrentHolderOfLadyOfTheLake };
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
            CurrentVoteTrack.Excalibur = new ExcaliburUse { Holder = proposedPlayer };

            GameState = State.VotingForTeam;
        }

        void UseExcalibur(Player player, Player proposedPlayer)
        {
            if (proposedPlayer!=null)
            {
                CurrentVoteTrack.Excalibur.UsedOn = CurrentVoteTrack.QuestCards.First(p => p.Player == proposedPlayer);
                CurrentVoteTrack.Excalibur.OriginalMissionWasSuccess = CurrentVoteTrack.Excalibur.UsedOn.Success;
                CurrentVoteTrack.Excalibur.UsedOn.Success = !CurrentVoteTrack.Excalibur.UsedOn.Success;
            }
            else
            {
                CurrentVoteTrack.Excalibur.Skipped = true;
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
                    Winners = Players.Where(p => IsCharacterEvil(p.Character, CurrentLancelotAllegianceSwitched)).ToList();
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
                if (CurrentVoteTrack.Excalibur != null)
                {
                    GameState = State.UsingExcalibur;
                }
                else
                {
                    QuestFinished();
                }
            }
        }

        void UseLadyOfTheLake(Player target)
        {
            CurrentQuest.LadyOfTheLake.Target = target;
            CurrentQuest.LadyOfTheLake.IsEvil = IsCharacterEvil(target.Character, CurrentLancelotAllegianceSwitched);
            CurrentHolderOfLadyOfTheLake = target;

            QuestFinished();
        }


    }
}
