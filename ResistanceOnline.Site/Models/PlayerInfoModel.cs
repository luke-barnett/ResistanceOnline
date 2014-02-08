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
        public bool IsEvil { get; set; }
        public bool CouldBeMerlin { get; set; }
        public Character? CharacterCard { get; set; }

        public string Actual
        {
            get
            {
                if (CharacterCard.HasValue)
                    return CharacterCard.ToString();
                if (IsEvil)
                    return "Evil";
                if (CouldBeMerlin)
                    return "Merlin";
                return "Player";
            }
        }
    }
}