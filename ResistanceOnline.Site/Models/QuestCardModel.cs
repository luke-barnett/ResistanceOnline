using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Site.Models
{
    public class QuestCardModel
    {
        public bool Hidden { get; set; }
        public bool Success { get; set; }

        public string Image
        {
            get
            {
                if (Hidden)
                    return "quest";
                if (Success)
                    return "questsuccess";
                return "questfail";
            }
        }

        public QuestCardModel(bool success, bool hidden)
        {
            Success = success;
            Hidden = hidden;
        }

    }
}
