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
            ActionOnly,
            List,
            FreeText
        };

        public Action.Type Action { get; set; }
        public AvailableAction.Type AvailableOptions { get; set; }
        public List<string> Options { get; set; }

        public static AvailableAction ActionOnly(Action.Type action)
        {
            return new AvailableAction { Action = action, AvailableOptions = Type.ActionOnly };
        }
        public static AvailableAction List(Action.Type action, List<string> options)
        {
            return new AvailableAction { Action = action, AvailableOptions = Type.List, Options = options };
        }
        public static AvailableAction FreeText(Action.Type action)
        {
            return new AvailableAction { Action = action, AvailableOptions = Type.FreeText };
        }
    }
}
