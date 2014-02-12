using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core.Bots
{
    public class SimpleBot : Player, IBot
    {
        public void DoSomething(Game game)
        {
            var availableActions = game.AvailableActions(this);
            if (availableActions.Count() == 0)
                return;

            var iAmEvil = game.IsCharacterEvil(this.Character);
            
            if (availableActions.Contains(Action.Type.VoteForTeam))
            {
                var evilCount = CountEvil(game.CurrentRound.CurrentTeam.TeamMembers, game);
                game.PerformAction(this, new Action { ActionType = Action.Type.VoteForTeam, Accept = ((evilCount > 0 && iAmEvil) || evilCount == 0 && !iAmEvil) });
                return;
            }

            if (availableActions.Contains(Action.Type.SubmitQuestCard))
            {
                game.PerformAction(this, new Action { ActionType = Action.Type.SubmitQuestCard, Success = iAmEvil == false });
            }

            if (availableActions.Contains(Action.Type.AddToTeam))
            {
                Player player;
                //put myself on mission
                if (!game.CurrentRound.CurrentTeam.TeamMembers.Any(p => p == this))
                {
                    player = this;
                }
                else
                {
                    if (iAmEvil)
                    {
                        //put others on mission
                        player = game.Players.Except(game.CurrentRound.CurrentTeam.TeamMembers).First();
                    }
                    else
                    {
                        //put people I know are not evil on mission
                        player = game.Players.Except(game.CurrentRound.CurrentTeam.TeamMembers).FirstOrDefault(p => p != this && IKnowTheyAreEvil(p, game) == false);

                        //failing that we need to put someone evil on the mission :(
                        if (player == null)
                            player = game.Players.Except(game.CurrentRound.CurrentTeam.TeamMembers).First();
                    }
                }
                game.PerformAction(this, new Action { ActionType = Action.Type.AddToTeam, Player = player});
            }

            if (availableActions.Contains(Action.Type.GuessMerlin))
            {
                var player = game.Players.FirstOrDefault(p => p != this && IKnowTheyAreEvil(p, game) == false);
                game.PerformAction(this, new Action { ActionType = Action.Type.GuessMerlin, Player = player });
            }

            if (availableActions.Contains(Action.Type.UseTheLadyOfTheLake))
            {
                Player target;
                if (game.LadyOfTheLakeUses.Count == 0)
                {
                    target = game.Players.First(p => p != this);
                }
                else
                {
                    target = game.LadyOfTheLakeUses.Select(u => u.UsedBy).Where(p => p != this).Except(game.CurrentRound.CurrentTeam.TeamMembers).First();
                }
                game.PerformAction(this, new Action { ActionType = Action.Type.UseTheLadyOfTheLake, Player = target });
            }
        }

        private int CountEvil(List<Player> list, Game game)
        {
            var evilCount = 0;
            foreach (var player in list)
            {
                if (IKnowTheyAreEvil(player,game))
                {
                    evilCount++;
                }
            }
            return evilCount;
        }

        private bool IKnowTheyAreEvil(Player player, Game game)
        {
            var knowledge = game.PlayerKnowledge(this, player);
            if (knowledge == Knowledge.Evil || (knowledge == Knowledge.EvilLancelot && !game.LancelotAllegianceSwitched) || (knowledge == Knowledge.Lancelot && game.LancelotAllegianceSwitched))
            {
                return true;
            }
            return false;
        }
    }
        
}
