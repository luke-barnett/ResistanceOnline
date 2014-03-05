using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class Game
    {
        public enum State
        {
            Setup,
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

        public State GameState { get; set; }
        public GameSetup Setup { get; set; }
        public List<PlayerMessage> Messages { get; set; }
        public List<Round> Rounds { get; set; }
        public int QuestIndicator { get; set; }
        public Player HolderOfLadyOfTheLake { get; set; }
        public bool LancelotAllegianceSwitched { get; set; }
        public Player AssassinsGuessAtMerlin { get; set; }
        public Round CurrentRound { get { return Rounds.LastOrDefault(); } }
        public Team CurrentTeam { get { return CurrentRound == null ? null : CurrentRound.CurrentTeam; } }   // todo - use the fancy "?." operator :)   

        public Game(GameSetup setup)
        {
            GameState = State.Setup;
            Rounds = new List<Round>();

            Setup = setup;
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
            if (Setup.Rules.Contains(Rule.LancelotsKnowEachOther))
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
            switch (GameState)
            {
                case State.Setup:
                    return new List<Action.Type>() { Action.Type.StartGame };
                    break;
                case State.ChoosingTeam:
                    if (player == CurrentTeam.Leader)
                    {
                        return new List<Action.Type>() { Action.Type.AddToTeam };
                    }
                    break;
                case State.AssigningExcalibur:
                    if (player == CurrentTeam.Leader)
                    {
                        return new List<Action.Type>() { Action.Type.AssignExcalibur };
                    }
                    break;
                case State.VotingForTeam:
                    if (!CurrentTeam.Votes.Any(v => v.Player == player))
                    {
                        return new List<Action.Type>() { Action.Type.VoteApprove, Action.Type.VoteReject };
                    }
                    break;
                case State.Questing:
                    if (CurrentTeam.TeamMembers.Contains(player) && !CurrentTeam.Quests.Any(q => q.Player == player))
                    {
                        //good must always vote success
                        if (Setup.Rules.Contains(Rule.GoodMustAlwaysVoteSucess) && !Setup.IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
                        {
                            return new List<Action.Type>() { Action.Type.SucceedQuest };
                        }
                        //lancelot fanatasism
                        if (Setup.Rules.Contains(Rule.LancelotsMustVoteFanatically) && (player.Character == Character.Lancelot || player.Character == Character.EvilLancelot))
                        {
                            if (Setup.IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
                            {
                                return new List<Action.Type>() { Action.Type.FailQuest };
                            }
                            else
                            {
                                return new List<Action.Type>() { Action.Type.SucceedQuest };
                            }
                        }

                        return new List<Action.Type>() { Action.Type.FailQuest, Action.Type.SucceedQuest };
                    }
                    break;
                case State.UsingExcalibur:
                    if (CurrentTeam.Excalibur.Holder == player)
                    {
                        return new List<Action.Type>() { Action.Type.UseExcalibur };
                    }
                    break;
                case State.LadyOfTheLake:
                    if (Setup.Rules.Contains(Rule.IncludeLadyOfTheLake) && CurrentRound.LadyOfTheLake != null && CurrentRound.LadyOfTheLake.Holder == player && CurrentRound.LadyOfTheLake.Target == null)
                    {
                        return new List<Action.Type>() { Action.Type.UseTheLadyOfTheLake };
                    }
                    break;
                case Game.State.GuessingMerlin:
                    if (player != null && player.Character == Character.Assassin)
                        return new List<Action.Type>() { Action.Type.GuessMerlin };
                    break;
            }
            return new List<Action.Type>();
        }

        public void DoActions(List<Action> actions)
        {
            foreach (var action in actions.Where(a=>a.GameId == Setup.GameId))
            {
                DoAction(action);
            }
        }

        public void DoAction(int gameId, Player sourcePlayer, Action.Type actionType, Player targetPlayer)
        {
            DoAction(new PlayerAction(gameId, sourcePlayer, actionType, targetPlayer));
        }

        public void DoAction(int gameId, Player sourcePlayer, Action.Type actionType)
        {                
            DoAction(new Action(gameId, sourcePlayer, actionType));
        }

        public void DoAction(Action action)
        {            
            if (action.GameId == Setup.GameId)
            {
                if (!AvailableActions(action.SourcePlayer).Contains(action.ActionType))
                {
                    throw new InvalidOperationException("Action not valid");
                }

                switch (action.ActionType)
                {
                    case Action.Type.StartGame:
                        StartGame();
                        break;
                    case Action.Type.AddToTeam:
                        AddToTeam(action.SourcePlayer, ((PlayerAction)action).TargetPlayer);
                        break;
                    case Action.Type.AssignExcalibur:
                        AssignExcalibur(action.SourcePlayer, ((PlayerAction)action).TargetPlayer);
                        break;
                    case Action.Type.FailQuest:
                        SubmitQuest(action.SourcePlayer, false);
                        break;
                    case Action.Type.GuessMerlin:
                        GuessMerlin(action.SourcePlayer, ((PlayerAction)action).TargetPlayer);
                        break;
                    case Action.Type.SucceedQuest:
                        SubmitQuest(action.SourcePlayer, true);
                        break;
                    case Action.Type.UseExcalibur:
                        UseExcalibur(action.SourcePlayer, ((PlayerAction)action).TargetPlayer);
                        break;
                    case Action.Type.UseTheLadyOfTheLake:
                        UseLadyOfTheLake(action.SourcePlayer, ((PlayerAction)action).TargetPlayer);
                        break;
                    case Action.Type.VoteApprove:
                        VoteForTeam(action.SourcePlayer, true);
                        break;
                    case Action.Type.VoteReject:
                        VoteForTeam(action.SourcePlayer, false);
                        break;
                }
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

        void StartGame()
        {
            if (GameState != State.Setup)
                throw new InvalidOperationException("Can only start game during setup");
           
            if (Setup.AvailableCharacters.Count != Setup.GameSize)
                throw new InvalidOperationException("Not Enough Characters for Players");

            Setup.AllocateCharacters();
            Setup.ChooseLeader();
            Setup.GameStarted = true;

            HolderOfLadyOfTheLake = Setup.InitialHolderOfLadyOfTheLake;
            NextRound(Setup.InitialLeader);
        }

        void NextRound(Player leader)
        {
            var roundTable = Setup.RoundTables[Rounds.Count];
            var round = new Round(Setup.Players, leader, HolderOfLadyOfTheLake, roundTable.TeamSize, roundTable.RequiredFails, LancelotAllegianceSwitched);
            var team = new Team(leader, roundTable.TeamSize, roundTable.RequiredFails);
            round.Teams.Add(team);
            Rounds.Add(round);
            GameState = State.ChoosingTeam;
        }

        void OnRoundFinished()
        {
            //3 failed missions, don't bother going any further
            if (Rounds.Count(r => !r.IsSuccess.Value) >= 3)
            {
                GameState = State.EvilTriumphs;
                return;
            }

            //3 successful missions, don't bother going any further
            if (Rounds.Count(r => r.IsSuccess.Value) >= 3)
            {
                if (Setup.AvailableCharacters.Contains(Character.Assassin))
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
            if (Setup.GetLoyaltyCard(Rounds.Count) == LoyaltyCard.SwitchAlegiance)
            {
                LancelotAllegianceSwitched = !LancelotAllegianceSwitched;
            }

            NextRound(Setup.Players.Next(CurrentRound.CurrentTeam.Leader));
        }


        void AddToTeam(Player player, Player proposedPlayer)
        {
            if (CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is already on the team");

            CurrentTeam.TeamMembers.Add(proposedPlayer);

            if (CurrentTeam.TeamMembers.Count == CurrentTeam.TeamSize)
            {
                if (Setup.Rules != null && Setup.Rules.Contains(Rule.IncludeExcalibur))
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
            if (!CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new InvalidOperationException("Player is not on team..");

            if (proposedPlayer == CurrentTeam.Leader)
                throw new InvalidOperationException("Leader cannot assign excalibur to themself");

            CurrentTeam.Excalibur.Holder = proposedPlayer;

            GameState = State.VotingForTeam;
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
            if (Setup.Rules.Contains(Rule.IncludeLadyOfTheLake) && Rounds.Count >= 2)
            {
                GameState = State.LadyOfTheLake;
                return;
            }

            OnRoundFinished();
        }

        void VoteForTeam(Player player, bool approve)
        {
            CurrentTeam.Votes.Add(new Vote { Approve = approve, Player = player });

            if (CurrentTeam.Votes.Count < Setup.GameSize)
                return;

            //on the last vote, if it fails, create the next quest
            var rejects = CurrentTeam.Votes.Count(v => !v.Approve);
            if (rejects >= Math.Ceiling(Setup.GameSize / 2.0))
            {
                if (CurrentRound.Teams.Count == 5)
                {
                    GameState = State.EternalChaos;
                }
                else
                {
                    CurrentRound.Teams.Add(new Team(Setup.Players.Next(CurrentTeam.Leader), CurrentRound.TeamSize, CurrentRound.RequiredFails));
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
            CurrentRound.LadyOfTheLake.IsEvil = Setup.IsCharacterEvil(target.Character, LancelotAllegianceSwitched);

            OnRoundFinished();
        }

    }
}
