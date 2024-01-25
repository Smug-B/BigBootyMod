using System.Collections.Generic;

namespace BigBootyMod.Core.Utils
{
    public static class DictionaryUtils
    {
        public static bool HasSafeValue<T, K>(this IDictionary<T, K> dictionary, T key) => dictionary.TryGetValue(key, out K? value) && value is K;
    }
}
