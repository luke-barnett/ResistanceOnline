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

        public Game(GameSetup setup)
        {
            GameState = State.Setup;
            Rounds = new List<Round>();

            Setup = setup;
        }

        public void DoActions(List<Action> actions)
        {
            foreach (var action in actions.Where(a=>a.GameId == Setup.GameId))
            {
                DoAction(action);
            }
        }

        public void DoAction(int gameId, Player sourcePlayer, Action.Type actionType, Player targetPlayer = null)
        {
            DoAction(new Action(gameId, sourcePlayer, actionType, targetPlayer));
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
                    case Action.Type.AddToTeam:
                        AddToTeam(action.SourcePlayer, action.TargetPlayer);
                        break;
                    case Action.Type.AssignExcalibur:
                        AssignExcalibur(action.SourcePlayer, action.TargetPlayer);
                        break;
                    case Action.Type.FailQuest:
                        SubmitQuest(action.SourcePlayer, false);
                        break;
                    case Action.Type.GuessMerlin:
                        GuessMerlin(action.SourcePlayer, action.TargetPlayer);
                        break;
                    case Action.Type.SucceedQuest:
                        SubmitQuest(action.SourcePlayer, true);
                        break;
                    case Action.Type.UseExcalibur:
                        UseExcalibur(action.SourcePlayer, action.TargetPlayer);
                        break;
                    case Action.Type.UseTheLadyOfTheLake:
                        UseLadyOfTheLake(action.SourcePlayer, action.TargetPlayer);
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
            if (GameState != State.GuessingMerlin)
            {
                throw new Exception("Hax. You shouldn't be guessing merlin at this stage");
            }

            if (player.Character != Character.Assassin)
            {
                throw new Exception("Hax. Player is not assassin.");
            }

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


        public void StartGame()
        {
            if (GameState != State.Setup)
                throw new Exception("Can only start game during setup");
           
            if (Setup.AvailableCharacters.Count != Setup.GameSize)
                throw new Exception("Not Enough Characters for Players");

            Setup.AllocateCharacters();
            Setup.ChooseLeader();

            HolderOfLadyOfTheLake = Setup.InitialHolderOfLadyOfTheLake;

            //create first round
            NextRound(Setup.InitialLeader);
        }



        public Round CurrentRound { get { return Rounds.LastOrDefault(); } }
        public Team CurrentTeam { get { return CurrentRound == null ? null : CurrentRound.CurrentTeam; } }   // todo - use the fancy "?." operator :)   


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
            var loyaltyCard = Setup.GetLoyaltyCard(Rounds.Count);
            if (loyaltyCard == LoyaltyCard.SwitchAlegiance)
            {
                LancelotAllegianceSwitched = !LancelotAllegianceSwitched;
            }

            NextRound(Setup.Players.Next(CurrentRound.CurrentTeam.Leader));
        }

        public List<Action.Type> AvailableActions(Player player)
        {
            switch (GameState)
            {
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

        void AddToTeam(Player player, Player proposedPlayer)
        {
            if (CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new Exception("Hax. Player is already on the team");

            if (player != CurrentTeam.Leader)
                throw new Exception("Hax. Player is not the leader of this team");

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
            if (!Setup.Rules.Contains(Rule.IncludeExcalibur))
            {
                throw new Exception("Game does not include excalibur");
            }

            if (player != CurrentTeam.Leader)
                throw new Exception("Hax. Player is not the leader of this team");

            if (!CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is not on team..");

            if (proposedPlayer == CurrentTeam.Leader)
                throw new Exception("Leader cannot assign excalibur to themself");

            CurrentTeam.Excalibur.Holder = proposedPlayer;

            GameState = State.VotingForTeam;
        }

        void UseExcalibur(Player player, Player proposedPlayer)
        {
            if (!Setup.Rules.Contains(Rule.IncludeExcalibur))
            {
                throw new Exception("Game does not include excalibur");
            }

            if (player != CurrentTeam.Excalibur.Holder)
                throw new Exception("Hax. Player does not have excalibur");

            if (proposedPlayer != null && !CurrentTeam.TeamMembers.Contains(proposedPlayer))
                throw new Exception("Player is not on team..");

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
            if (CurrentTeam.Votes.Any(v => v.Player == player))
                throw new Exception("Player has already voted");

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
            if (Setup.Rules.Contains(Rule.GoodMustAlwaysVoteSucess) && !success && !Setup.IsCharacterEvil(player.Character, LancelotAllegianceSwitched))
            {
                throw new Exception("Good must always vote success");
            }
            if (Setup.Rules.Contains(Rule.LancelotsMustVoteFanatically) && (player.Character == Character.Lancelot || player.Character == Character.EvilLancelot))
            {
                if ((success && Setup.IsCharacterEvil(player.Character, LancelotAllegianceSwitched)) || (!success && !Setup.IsCharacterEvil(player.Character, LancelotAllegianceSwitched)))
                {
                    throw new Exception("Lancelot must move fanatically");
                }
            }

            if (CurrentTeam.Quests.Select(v => v.Player).ToList().Contains(player))
                throw new Exception("Player has already submitted their quest card..");

            CurrentTeam.Quests.Add(new Quest { Player = player, Success = success });

            if (CurrentTeam.Quests.Count == CurrentTeam.TeamSize)
            {
                OnTeamFinished();
            }
        }

        void UseLadyOfTheLake(Player player, Player target)
        {
            if (GameState != State.LadyOfTheLake)
                throw new Exception("Now is not the time for funny business with ladies");

            if (CurrentRound.LadyOfTheLake != null && CurrentRound.LadyOfTheLake.Holder != player)
                throw new Exception("Hax. Player does not have lady of the lake.");

            CurrentRound.LadyOfTheLake.Target = target;
            CurrentRound.LadyOfTheLake.IsEvil = Setup.IsCharacterEvil(target.Character, LancelotAllegianceSwitched);

            OnRoundFinished();
        }

    }
}
