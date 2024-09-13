using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine
{
    internal static class Extensions
    {
        public static T GetOrDefault<T>(this T[] array, int index) where T : struct
        {
            if (index < array.Length)
                return array[index];
            else
                return default;
        }

        public static void QuickRemoveAt<T>(this List<T> list, int index)
        {
            list[index] = list[^1];
            list.RemoveAt(list.Count-1);
        }

        public static void RemoveLast<T>(this List<T> list)
        {
            list.RemoveAt(list.Count-1);
        }

        /// <summary>
        /// Partitions list so matched elements appear before all unmatched elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="matchFunc"></param>
        /// <returns>The count of matched elements in list</returns>
        public static int PartitionBy<T>(this IList<T> list, Func<T, bool> matchFunc)
        {
            return list.PartitionBy(0, list.Count, matchFunc);
        }

        /// <summary>
        /// Partitions list so matched elements appear before all unmatched elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="start"></param>
        /// <param name="matchFunc"></param>
        /// <returns>The count of matched elements in list</returns>
        public static int PartitionBy<T>(this IList<T> list, int start, Func<T, bool> matchFunc)
        {
            var length = list.Count - start;
            return list.PartitionBy(start, length, matchFunc);
        }

        /// <summary>
        /// Partitions list so matched elements appear before all unmatched elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="matchFunc"></param>
        /// <returns>The count of matched elements in list</returns>
        public static int PartitionBy<T>(this IList<T> list, int start, int count, Func<T, bool> matchFunc)
        {
            int first = start;
            int last = start + count - 1;

            while (true)
            {
                while (first <= last && matchFunc(list[first]))
                    first++;

                // if we break here, "first" is on the first false
                if (first >= last)
                    break;

                while (first <= last && !matchFunc(list[last]))
                    last--;

                // if we break here, "first" is still sitting on the first false
                if (first >= last)
                    break;

                // swap
                T tmp = list[first];
                list[first] = list[last];
                list[last] = tmp;
            }

            // "first" is sitting on the first false
            return first - start;
        }

        /// <summary>
        /// Partitions list so matched elements appear before all unmatched elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="matchFunc"></param>
        /// <returns>The count of matched elements in list</returns>
        public static int PartitionBy<T>(this Span<T> list, Func<T, bool> matchFunc)
        {
            return list.PartitionBy(0, list.Length, matchFunc);
        }

        /// <summary>
        /// Partitions list so matched elements appear before all unmatched elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="start"></param>
        /// <param name="matchFunc"></param>
        /// <returns>The count of matched elements in list</returns>
        public static int PartitionBy<T>(this Span<T> list, int start, Func<T, bool> matchFunc)
        {
            var length = list.Length - start;
            return list.PartitionBy(start, length, matchFunc);
        }

        /// <summary>
        /// Partitions list so matched elements appear before all unmatched elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="matchFunc"></param>
        /// <returns>The count of matched elements in list</returns>
        public static int PartitionBy<T>(this Span<T> list, int start, int count, Func<T, bool> matchFunc)
        {
            int first = start;
            int last = start + count - 1;

            while (true)
            {
                while (first <= last && matchFunc(list[first]))
                    first++;

                // if we break here, "first" is on the first false
                if (first >= last)
                    break;

                while (first <= last && !matchFunc(list[last]))
                    last--;

                // if we break here, "first" is still sitting on the first false
                if (first >= last)
                    break;

                // swap
                T tmp = list[first];
                list[first] = list[last];
                list[last] = tmp;
            }

            // "first" is sitting on the first false
            return first - start;
        }
    }
}
