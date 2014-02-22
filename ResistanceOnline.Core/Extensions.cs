using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Core
{
    public static class Extensions
    {
        private static Random _random = new Random();

        /// <summary>
        /// returns a random element from the list
        /// </summary>
        public static T Random<T>(this IEnumerable<T> list)
        {
            return list.Random(x => true);
        }

        /// <summary>
        /// returns a random element from the list
        /// </summary>
        public static T Random<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            return (T)list.OrderBy(x => _random.Next()).First(predicate);
        }

        /// <summary>
        /// returns a random element from the list
        /// </summary>
        public static T RandomOrDefault<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            return (T)list.OrderBy(x => _random.Next()).FirstOrDefault(predicate);
        }

        /// <summary>
        /// returns a random element from the list
        /// </summary>
        public static T RandomOrDefault<T>(this IEnumerable<T> list)
        {
            return list.RandomOrDefault(x => true);
        }

        /// <summary>
        /// shuffles the list randomly
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
        {
            return list.OrderBy(x => _random.Next());
        }

        /// <summary>
        /// adds a number after the string until it's unique within the given scope
        /// </summary>
        public static string Uniquify(this string s, IEnumerable<string> scope)
        {
            var i = 2;
            var r = s;
            while (scope.Contains(r))
            {
                r = string.Format("{0} {1}", s, i);
                i++;
            }
            return r;
        }

    }
}