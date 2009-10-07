using System;
using System.Collections.Generic;

namespace CDP.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> self, IDictionary<TKey, TValue> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                self.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
