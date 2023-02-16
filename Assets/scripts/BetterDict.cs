using System.Collections.Generic;

public static class BetterDict
{
    public static void AddOrCreate<TKey, TCollection, TValue>(
        this Dictionary<TKey, TCollection> dictionary, TKey key, TValue value)
        where TCollection : ICollection<TValue>, new()
    {
        TCollection collection;
        if (!dictionary.TryGetValue(key, out collection))
        {
            collection = new TCollection();
            dictionary.Add(key, collection);
        }
        collection.Add(value);
    }
}