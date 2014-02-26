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
        /// returns the next element in the list with rotating back to the start as appropriate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static T Next<T>(this List<T> list, T current)
        {
            return list[(list.IndexOf(current) +1) % list.Count];
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