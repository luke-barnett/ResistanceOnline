using ResistanceOnline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Site.Models
{
    public class PlayerInfoModel
    {
        public string Name { get; set; }
        public Character? CharacterCard { get; set; }

        public string Actual
        {
            get
            {
                if (CharacterCard.HasValue)
                    return CharacterCard.ToString();
                return Knowledge.ToString();
            }
        }

        public Knowledge Knowledge { get; set; }
    }
}