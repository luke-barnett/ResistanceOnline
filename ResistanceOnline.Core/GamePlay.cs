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
    public class GamePlay
    {
        public enum State
        {
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

        public State GamePlayState { get; set; }
        public Game Game { get; set; }
        public List<Quest> Quests { get; set; }
        public Player HolderOfLadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }
        public Quest CurrentQuest { get { return Quests.LastOrDefault(); } }
        public VoteTrack CurrentVoteTrack { get { return CurrentQuest == null ? null : CurrentQuest.CurrentVoteTrack; } }   // todo - use the fancy "?." operator :)   

        public GamePlay(Game game)
        {
            Quests = new List<Quest>();
            Game = game;
            HolderOfLadyOfTheLake = Game.InitialHolderOfLadyOfTheLake;
            NextQuest(Game.InitialLeader);
        }

        public void Message(Player player, string message)
        {
            CurrentQuest.CurrentVoteTrack.Messages.Add(new PlayerMessage { Player = player, Message = message });
        }

        public Knowledge PlayerKnowledge(Player myself, Player someoneelse)
        {
            if (myself == null)
                return Knowledge.Player;

            //lancelots can know each other
            if (Game.Rules.Contains(Rule.LancelotsKnowEachOther))
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
            switch (GamePlayState)
            {
                case State.ChoosingTeam:
                    if (player == CurrentVoteTrack.Leader)
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
                        if (Game.Rules.Contains(Rule.GoodMustAlwaysSucceedQuests) && !Game.IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
                        {
                            actions.Add(Action.Type.SucceedQuest);
                        }
                        else
                        {
                            //lancelot fanatasism
                            if (Game.Rules.Contains(Rule.LancelotsMustVoteFanatically) && (player.Character == Character.Lancelot || player.Character == Character.EvilLancelot))
                            {
                                if (Game.IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
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
                    if (Game.Rules.Contains(Rule.LadyOfTheLakeExists) && CurrentQuest.LadyOfTheLake != null && CurrentQuest.LadyOfTheLake.Holder == player && CurrentQuest.LadyOfTheLake.Target == null)
                    {
                        actions.Add(Action.Type.UseTheLadyOfTheLake);
                    }
                    break;
                case GamePlay.State.GuessingMerlin:
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

        public void DoAction(int gameId, Player sourcePlayer, Action.Type actionType, Player targetPlayer)
        {
            DoAction(new Action(sourcePlayer, actionType, targetPlayer));
        }

        public void DoAction(int gameId, Player sourcePlayer, Action.Type actionType)
        {                
            DoAction(new Action(sourcePlayer, actionType));
        }

        public void DoAction(Action action)
        {
            if (!AvailableActions(action.Owner).Contains(action.ActionType))
            {
                throw new InvalidOperationException("Action not valid");
            }

            switch (action.ActionType)
            {
                case Action.Type.Message:
                    Message(action.Owner, action.Text);
                    break;
                case Action.Type.AddToTeam:
                    AddToTeam(action.Owner, action.TargetPlayer);
                    break;
                case Action.Type.AssignExcalibur:
                    AssignExcalibur(action.Owner, action.TargetPlayer);
                    break;
                case Action.Type.FailQuest:
                    SubmitQuest(action.Owner, false);
                    break;
                case Action.Type.GuessMerlin:
                    GuessMerlin(action.Owner, action.TargetPlayer);
                    break;
                case Action.Type.SucceedQuest:
                    SubmitQuest(action.Owner, true);
                    break;
                case Action.Type.UseExcalibur:
                    UseExcalibur(action.Owner, action.TargetPlayer);
                    break;
                case Action.Type.UseTheLadyOfTheLake:
                    UseLadyOfTheLake(action.Owner, action.TargetPlayer);
                    break;
                case Action.Type.VoteApprove:
                    VoteForTeam(action.Owner, true);
                    break;
                case Action.Type.VoteReject:
                    VoteForTeam(action.Owner, false);
                    break;
            }
        }

        void GuessMerlin(Player player, Player guess)
        {
            AssassinsGuessAtMerlin = guess;

            if (guess.Character == Character.Merlin)
            {
                GamePlayState = State.EvilTriumphs;
            }
            else
            {
                GamePlayState = State.GoodPrevails;
            }
        }

        void NextQuest(Player leader)
        {
            var roundTable = Game.RoundTables[Quests.Count];
            var round = new Quest(Game.Players, leader, HolderOfLadyOfTheLake, roundTable.TeamSize, roundTable.RequiredFails, LancelotAllegianceSwitched);
            var team = new VoteTrack(leader, roundTable.TeamSize, roundTable.RequiredFails);
            round.VoteTracks.Add(team);
            Quests.Add(round);
            GamePlayState = State.ChoosingTeam;
        }

        void QuestFinished()
        {
            //3 failed missions, don't bother going any further
            if (Quests.Count(r => !r.IsSuccess.Value) >= 3)
            {
                GamePlayState = State.EvilTriumphs;
                return;
            }

            //3 successful missions, don't bother going any further
            if (Quests.Count(r => r.IsSuccess.Value) >= 3)
            {
                if (Game.AvailableCharacters.Contains(Character.Assassin))
                {
                    GamePlayState = State.GuessingMerlin;
                }
                else
                {
                    GamePlayState = State.GoodPrevails;
                }
                return;
            }

            //loyalty cards
            if (Game.GetLoyaltyCard(Quests.Count) == LoyaltyCard.SwitchAlegiance)
            {
                LancelotAllegianceSwitched = !LancelotAllegianceSwitched;
            }

            NextQuest(Game.Players.Next(CurrentQuest.CurrentVoteTrack.Leader));
        }


        void AddToTeam(Player player, Player proposedPlayer)
        {
            if (CurrentVoteTrack.Players.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is already on the team");

            CurrentVoteTrack.Players.Add(proposedPlayer);

            if (CurrentVoteTrack.Players.Count == CurrentVoteTrack.QuestSize)
            {
                if (Game.Rules != null && Game.Rules.Contains(Rule.ExcaliburExists))
                {
                    GamePlayState = State.AssigningExcalibur;
                }
                else
                {
                    GamePlayState = State.VotingForTeam;
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

            GamePlayState = State.VotingForTeam;
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
            if (Game.Rules.Contains(Rule.LadyOfTheLakeExists) && Quests.Count >= 2)
            {
                GamePlayState = State.LadyOfTheLake;
                return;
            }

            QuestFinished();
        }

        void VoteForTeam(Player player, bool approve)
        {
            CurrentVoteTrack.Votes.Add(new VoteToken { Approve = approve, Player = player });

            if (CurrentVoteTrack.Votes.Count < Game.Players.Count)
                return;

            //on the last vote, if it fails, create the next quest
            var rejects = CurrentVoteTrack.Votes.Count(v => !v.Approve);
            if (rejects >= Math.Ceiling(Game.Players.Count / 2.0))
            {
                if (CurrentQuest.VoteTracks.Count == 5)
                {
                    GamePlayState = State.EternalChaos;
                }
                else
                {
                    CurrentQuest.VoteTracks.Add(new VoteTrack(Game.Players.Next(CurrentVoteTrack.Leader), CurrentQuest.TeamSize, CurrentQuest.RequiredFails));
                    GamePlayState = State.ChoosingTeam;
                }
            }
            else
            {
                GamePlayState = State.Questing;
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
            if (Quests.Any(r=>r.LadyOfTheLake != null && r.LadyOfTheLake.Holder == target))
                throw new Exception("Once a lady has gone " + target + ", she does NOT go back..");

            CurrentQuest.LadyOfTheLake.Target = target;
            CurrentQuest.LadyOfTheLake.IsEvil = Game.IsCharacterEvil(target.Character, LancelotAllegianceSwitched);
            HolderOfLadyOfTheLake = target;

            QuestFinished();
        }

    }
}
