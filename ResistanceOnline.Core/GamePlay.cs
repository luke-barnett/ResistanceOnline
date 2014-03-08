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
        public List<Round> Rounds { get; set; }
        public int QuestIndicator { get; set; }
        public Player HolderOfLadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }
        public Round CurrentRound { get { return Rounds.LastOrDefault(); } }
        public Team CurrentTeam { get { return CurrentRound == null ? null : CurrentRound.CurrentTeam; } }   // todo - use the fancy "?." operator :)   

        public GamePlay(Game game)
        {
            Rounds = new List<Round>();
            Game = game;
            HolderOfLadyOfTheLake = Game.InitialHolderOfLadyOfTheLake;
            NextRound(Game.InitialLeader);
        }

        public void Message(Player player, string message)
        {
            CurrentRound.CurrentTeam.Messages.Add(new PlayerMessage { Player = player, Message = message });
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
                    if (player == CurrentTeam.Leader)
                    {
                        actions.Add(Action.Type.AddToTeam);
                    }
                    break;
                case State.AssigningExcalibur:
                    if (player == CurrentTeam.Leader)
                    {
                        actions.Add(Action.Type.AssignExcalibur);
                    }
                    break;
                case State.VotingForTeam:
                    if (!CurrentTeam.Votes.Any(v => v.Player == player))
                    {
                        actions.Add(Action.Type.VoteApprove);
                        actions.Add(Action.Type.VoteReject);
                    }
                    break;
                case State.Questing:
                    if (CurrentTeam.TeamMembers.Contains(player) && !CurrentTeam.Quests.Any(q => q.Player == player))
                    {
                        //good must always vote success
                        if (Game.Rules.Contains(Rule.GoodMustAlwaysVoteSucess) && !Game.IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
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
                    if (CurrentTeam.Excalibur.Holder == player)
                    {
                        actions.Add(Action.Type.UseExcalibur);
                    }
                    break;
                case State.LadyOfTheLake:
                    if (Game.Rules.Contains(Rule.IncludeLadyOfTheLake) && CurrentRound.LadyOfTheLake != null && CurrentRound.LadyOfTheLake.Holder == player && CurrentRound.LadyOfTheLake.Target == null)
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

        void NextRound(Player leader)
        {
            var roundTable = Game.RoundTables[Rounds.Count];
            var round = new Round(Game.Players, leader, HolderOfLadyOfTheLake, roundTable.TeamSize, roundTable.RequiredFails, LancelotAllegianceSwitched);
            var team = new Team(leader, roundTable.TeamSize, roundTable.RequiredFails);
            round.Teams.Add(team);
            Rounds.Add(round);
            GamePlayState = State.ChoosingTeam;
        }

        void OnRoundFinished()
        {
            //3 failed missions, don't bother going any further
            if (Rounds.Count(r => !r.IsSuccess.Value) >= 3)
            {
                GamePlayState = State.EvilTriumphs;
                return;
            }

            //3 successful missions, don't bother going any further
            if (Rounds.Count(r => r.IsSuccess.Value) >= 3)
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
            if (Game.GetLoyaltyCard(Rounds.Count) == LoyaltyCard.SwitchAlegiance)
            {
                LancelotAllegianceSwitched = !LancelotAllegianceSwitched;
            }

            NextRound(Game.Players.Next(CurrentRound.CurrentTeam.Leader));
        }


        void AddToTeam(Player player, Player proposedPlayer)
        {
            if (CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is already on the team");

            CurrentTeam.TeamMembers.Add(proposedPlayer);

            if (CurrentTeam.TeamMembers.Count == CurrentTeam.TeamSize)
            {
                if (Game.Rules != null && Game.Rules.Contains(Rule.IncludeExcalibur))
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
            if (!CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is not on team..");

            if (proposedPlayer == CurrentTeam.Leader)
                throw new InvalidOperationException("Leader cannot assign excalibur to themself");

            CurrentTeam.Excalibur.Holder = proposedPlayer;

            GamePlayState = State.VotingForTeam;
        }

        void UseExcalibur(Player player, Player proposedPlayer)
        {
            if (proposedPlayer != null && !CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is not on team..");

            if (proposedPlayer == player)
                throw new InvalidOperationException("Really? On yourself? How would that work? It's a sword..");

            CurrentTeam.Excalibur.UsedOn = CurrentTeam.Quests.First(p => p.Player == proposedPlayer);
            CurrentTeam.Excalibur.OriginalMissionWasSuccess = CurrentTeam.Excalibur.UsedOn.Success;
            CurrentTeam.Excalibur.UsedOn.Success = !CurrentTeam.Excalibur.UsedOn.Success;

            OnTeamFinished();
        }

        void OnTeamFinished()
        {
            if (Game.Rules.Contains(Rule.IncludeLadyOfTheLake) && Rounds.Count >= 2)
            {
                GamePlayState = State.LadyOfTheLake;
                return;
            }

            OnRoundFinished();
        }

        void VoteForTeam(Player player, bool approve)
        {
            CurrentTeam.Votes.Add(new Vote { Approve = approve, Player = player });

            if (CurrentTeam.Votes.Count < Game.Players.Count)
                return;

            //on the last vote, if it fails, create the next quest
            var rejects = CurrentTeam.Votes.Count(v => !v.Approve);
            if (rejects >= Math.Ceiling(Game.Players.Count / 2.0))
            {
                if (CurrentRound.Teams.Count == 5)
                {
                    GamePlayState = State.EternalChaos;
                }
                else
                {
                    CurrentRound.Teams.Add(new Team(Game.Players.Next(CurrentTeam.Leader), CurrentRound.TeamSize, CurrentRound.RequiredFails));
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
            CurrentTeam.Quests.Add(new Quest { Player = player, Success = success });

            if (CurrentTeam.Quests.Count == CurrentTeam.TeamSize)
            {
                OnTeamFinished();
            }
        }

        void UseLadyOfTheLake(Player player, Player target)
        {
            if (Rounds.Any(r=>r.LadyOfTheLake != null && r.LadyOfTheLake.Holder == target))
                throw new Exception("Once a lady has gone " + target + ", she does NOT go back..");

            CurrentRound.LadyOfTheLake.Target = target;
            CurrentRound.LadyOfTheLake.IsEvil = Game.IsCharacterEvil(target.Character, LancelotAllegianceSwitched);
            HolderOfLadyOfTheLake = target;

            OnRoundFinished();
        }

    }
}
