using System;
using System.Collections.Generic;

namespace NexusForever.Shared
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Shared.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}
