using System;
using System.Collections.Generic;
using System.Linq;

namespace DDD.Scripts.Game.rock_paper_scissors
{
    public static class ListExtensions
    {
        private static Random random = new Random();

        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List is null or empty");
            
            return list[random.Next(list.Count)];
        }

        public static List<T> GetRandomUnique<T>(this List<T> list, int count)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List is null or empty");
            
            if (count > list.Count)
                throw new ArgumentException("Requested count is larger than list size");

            return list.OrderBy(x => random.Next())
                .Take(count)
                .ToList();
        }
    }
}