using Diffusion.Database;
using System.Collections.Generic;

namespace Diffusion.Analysis;

public class Vocabulary
{
    private readonly OrderedSet<string> _vocabularySet = new OrderedSet<string>();

    public int Count => _vocabularySet.Count;

    public void AddToken(string token)
    {
        _vocabularySet.Add(token);
    }

    public void SetDictionary(IEnumerable<IndexedItem<string>> dictionary)
    {
        _vocabularySet.SetItems(dictionary);
    }

    public IEnumerable<IndexedItem<string>> GetDictionary()
    {
        return _vocabularySet.GetItems();
    }

    /// <summary>
    /// Use the hashcode to uniquely identify the Vocabulary.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        var items = _vocabularySet.GetItems();

        unchecked
        {
            int hash = 17;

            foreach (var item in items.OrderBy(item => item.Index))
            {
                hash = hash * 23 + item.GetHashCode();
            }

            return hash;
        }

    }

    public void TokenizeAndAdd(IEnumerable<string> texts)
    {
        foreach (var text in texts)
        {
            var tokens = Tokenize(text);

            foreach (var token in tokens)
            {
                AddToken(token);
            }
        }

    }

    public IEnumerable<string> Tokenize(string text)
    {
        return text.Split(new char[] { ' ', ',' });
    }

    /// <summary>
    /// Returns a 1-hot encoding of each token in this vocabulary
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    public IEnumerable<int> Encode(string prompt)
    {
        var tokens = Tokenize(prompt);

        foreach (var token in tokens)
        {
            if (_vocabularySet.GetIndex(token, out int index))
            {
                yield return index;
            }
        }
    }
}
