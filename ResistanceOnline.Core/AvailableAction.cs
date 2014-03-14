using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    public class AvailableAction
    {
        public enum Type
        {
            Action,
            Items,
            FreeText
        };

        public Action.Type ActionType { get; set; }
        internal AvailableAction.Type AvailableActionType { get; set; }
        public List<string> ActionItems { get; set; }

        public static AvailableAction Action(Action.Type action)
        {
            return new AvailableAction { ActionType = action, AvailableActionType = Type.Action };
        }
        public static AvailableAction Items(Action.Type action, List<string> items)
        {
            return new AvailableAction { ActionType = action, AvailableActionType = Type.Items, ActionItems = items };
        }
        public static AvailableAction FreeText(Action.Type action)
        {
            return new AvailableAction { ActionType = action, AvailableActionType = Type.FreeText };
        }
    }
}
