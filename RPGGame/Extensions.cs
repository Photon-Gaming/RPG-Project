using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGGame
{
    public static class Extensions
    {
        private static readonly Random rng = new();

        private static T GetRandomItem<T>(List<T> list)
        {
            return list[rng.Next(0, list.Count)];
        }

        public static T ChooseRandom<T>(this IEnumerable<T> enumerable)
        {
            List<T> list = enumerable.ToList();

            if (list.Count == 0)
            {
                throw new ArgumentException("Enumerable is empty.");
            }

            return GetRandomItem(list);
        }

        public static T? ChooseRandomOrDefault<T>(this IEnumerable<T> enumerable)
        {
            List<T> list = enumerable.ToList();

            return list.Count == 0 ? default : GetRandomItem(list);
        }

        public static T? ChooseRandomOrDefault<T>(this IEnumerable<T> enumerable, T? defaultValue)
        {
            List<T> list = enumerable.ToList();

            return list.Count == 0 ? defaultValue : GetRandomItem(list);
        }
    }
}
