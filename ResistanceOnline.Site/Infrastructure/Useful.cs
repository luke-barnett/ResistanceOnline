using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Site.Infrastructure
{
    public static class Useful
    {
        public static string CommaQuibbling(IEnumerable<string> items)
        {
            var itemArray = items.ToArray();

            var commaSeparated = String.Join(", ", itemArray, 0, Math.Max(itemArray.Length - 1, 0));
            if (commaSeparated.Length > 0) commaSeparated += " and ";

            return commaSeparated + itemArray.LastOrDefault();
        }

    }
}