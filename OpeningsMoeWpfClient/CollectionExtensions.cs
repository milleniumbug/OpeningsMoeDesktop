using System;
using System.Collections.Generic;
using System.Linq;

namespace OpeningsMoeWpfClient
{
    static class CollectionUtils
    {
        public static void Shuffle<T>(IList<T> list, Random rand)
        {
            int n = list.Count;
            while(n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IList<T> Shuffled<T>(IList<T> list, Random rand)
        {
            var shuffledList = list.ToList();
            Shuffle(shuffledList, rand);
            return shuffledList;
        }

        public static IEnumerable<T> Cycle<T>(ICollection<T> collection)
        {
            while(true)
            {
                using(var enumerator = collection.GetEnumerator())
                {
                    while(enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }
            }
        }

        public static void ReplaceWith<T>(ICollection<T> targetCollection, IEnumerable<T> elements)
        {
            targetCollection.Clear();
            foreach(var element in elements)
            {
                targetCollection.Add(element);
            }
        }
    }
}
