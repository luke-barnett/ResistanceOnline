using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Site.Models
{
    public class WaitingActionsModel
    {
        public string Name { get; set; }
        public Core.Action.Type Action { get; set; }
    }
}