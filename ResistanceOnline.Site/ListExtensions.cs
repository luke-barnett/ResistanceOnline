using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Site
{
    public static class ListExtensions
    {
        public static T Random<T>(this IEnumerable<T> list)
        {
            return list.Random(x => true);
        }

        public static T Random<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            return (T)list.OrderBy(x => new Random().Next()).First(predicate);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
        {
            return list.OrderBy(x => new Random().Next());
        }


    }
}