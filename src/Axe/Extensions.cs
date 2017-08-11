using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe
{
    public static class Extensions
    {
        private static readonly Random rand = new Random();
        /// <summary>
        /// Generates a random permutation of list items using Fisher–Yates algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <see cref="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle"/>
        /// <returns></returns>
        public static IList<T> Shuffle<T>(this IList<T> items)
        {
            if (items != null && items.Count > 1)
            {
                for (int i = 0; i < items.Count - 1; i++)
                {
                    int j = rand.Next(i, items.Count);

                    T swap = items[i];
                    items[i] = items[j];
                    items[j] = swap;
                }
            }

            return items;
        }
    }
}
