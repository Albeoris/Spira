﻿using System.Collections.Generic;

namespace Spira.Core
{
    public static class IDictionaryExm
    {
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            TValue value;
            return dic.TryGetValue(key, out value) ? value : default(TValue);
        }
    }
}