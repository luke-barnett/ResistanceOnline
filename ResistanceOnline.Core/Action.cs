﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    /// <summary>
    /// this could probably be better as a base class with classes for different actions
    /// </summary>
    public class Action
    {
        public enum Type
        {
            [Description("Add a player to the current team")]
            AddToTeam,
            [Description("Approve the current team")]
            VoteApprove,
            [Description("Reject the current team")]
            VoteReject,
            [Description("Succeed the quest")]
            SucceedQuest,
            [Description("Fail the quest")]
            FailQuest,
            [Description("Try to assassinate Merlin")]
            GuessMerlin,
            [Description("Use the Lady of the Lake")]
            UseTheLadyOfTheLake,
            [Description("Assign Excalibur to team member")]
            AssignExcalibur,
            [Description("Use Excalibur on a quest card")]
            UseExcalibur,
            Message
        };

        public int GameId { get; set; }
        public Player Owner { get; set; }
        public Type ActionType { get; set; }
        public Player TargetPlayer { get; set; }
        public string Text { get; set; }

        public Action(Player owner, Type actionType, Player targetPlayer = null, string text = null)
        {
            Owner = owner;
            ActionType = actionType;
            TargetPlayer = targetPlayer;
            Text = text;
        }
       
    }


}
