// See https://aka.ms/new-console-template for more information

using Diffusion.Database;
using System.Collections;
using SparseBitsets;


var dataStore = new DataStore(@"C:\Users\ruper\AppData\Roaming\DiffusionToolkit\diffusion-toolkit.db");

var prompts = dataStore.GetImagePrompts().ToList();

var vocabulary = new OrderedSet<string>();

var i = 0;
var c = 0;

foreach (var imagePrompt in prompts)
{
    i++;
    if (imagePrompt.Prompt != null)
    {
        var tokens = imagePrompt.Prompt.Split(new char[] { ' ', ',' });

        foreach (var token in tokens)
        {
            c++;
            vocabulary.Add(token);
        }
    }
}

Console.WriteLine($"{i} prompts, {c} tokens");

Console.WriteLine($"{vocabulary.Count} unique tokens");

var promptEncodings = new List<PromptBitset>();

foreach (var imagePrompt in prompts)
{
    if (imagePrompt.Prompt != null)
    {
        var tokens = imagePrompt.Prompt.Split(new char[] { ' ', ',' });

        var bitset = new SparseBitset();

        foreach (var token in tokens)
        {
            if (vocabulary.GetIndex(token, out int index))
            {
                bitset.Add((ulong)index);
            }
        }

        bitset.Pack();

        promptEncodings.Add(new PromptBitset()
        {
            Id = imagePrompt.Id,
            Prompt = imagePrompt.Prompt,
            Bitset = bitset
        });

    }
}

while (true)
{
    Console.Clear();

    var m = Random.Shared.Next(0, promptEncodings.Count);

    var baseline = promptEncodings[m];

    var results = new List<(float, string)>();

    foreach (var pe in promptEncodings)
    {
        var index = CalculateJaccardIndex(baseline.Bitset, pe.Bitset);

        results.Add(new(index, pe.Prompt));

    }

    Console.WriteLine(baseline.Prompt);

    Console.WriteLine("Top 5");

    foreach (var result in results.Where(tuple => tuple.Item1 < 0.7 && tuple.Item1 > 0.5).DistinctBy(f => f.Item2).Take(5))
    {
        Console.WriteLine($"{result.Item1}: {result.Item2}");
    }


    //Console.WriteLine("Top 5");

    //foreach (var result in results.Where(tuple => tuple.Item1 > 0.9).Take(5))
    //{
    //    Console.WriteLine($"{result.Item1}: {result.Item2}");
    //}



    //Console.WriteLine("Bottom 5");

    //foreach (var result in results.Where(tuple => tuple.Item1 < 0.1).Take(5))
    //{
    //    Console.WriteLine($"{result.Item1}: {result.Item2}");
    //}

    Console.ReadLine();
}


Console.WriteLine("Done");

float CalculateJaccardIndex(SparseBitset a, SparseBitset b)
{
    var intersection = a.And(b);
    var union = a.Or(b);

    var index = intersection.GetPopCount() / (float)union.GetPopCount();

    return index;
}

public class PromptBitset
{
    public int Id { get; set; }
    public string Prompt { get; set; }
    public SparseBitset Bitset { get; set; }
}

public class IndexedItem<T>
{
    public int Index { get; set; }
    public T Value { get; set; }

    public IndexedItem(T value, int index)
    {
        this.Index = index;
        this.Value = value;
    }
}

public class OrderedSet<T> : ICollection<T>
{
    private readonly IDictionary<T, LinkedListNode<IndexedItem<T>>> m_Dictionary;
    private readonly LinkedList<IndexedItem<T>> m_LinkedList;

    public OrderedSet()
        : this(EqualityComparer<T>.Default)
    {
    }

    public OrderedSet(IEqualityComparer<T> comparer)
    {
        m_Dictionary = new Dictionary<T, LinkedListNode<IndexedItem<T>>>(comparer);
        m_LinkedList = new LinkedList<IndexedItem<T>>();
    }

    public int Count => m_Dictionary.Count;

    public virtual bool IsReadOnly => m_Dictionary.IsReadOnly;

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public bool Add(T item)
    {
        if (m_Dictionary.ContainsKey(item)) return false;
        var index = m_LinkedList.Count;
        var node = m_LinkedList.AddLast(new IndexedItem<T>(item, index));
        m_Dictionary.Add(item, node);
        return true;
    }

    public void Clear()
    {
        m_LinkedList.Clear();
        m_Dictionary.Clear();
    }

    public bool GetIndex(T item, out int index)
    {
        index = -1;

        var found = m_Dictionary.TryGetValue(item, out var node);
        if (!found) return false;

        index = node.Value.Index;

        return true;

    }

    public bool Remove(T item)
    {
        if (item == null) return false;
        var found = m_Dictionary.TryGetValue(item, out var node);
        if (!found) return false;
        m_Dictionary.Remove(item);
        m_LinkedList.Remove(node);
        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return m_LinkedList.Select(t => t.Value).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(T item)
    {
        return item != null && m_Dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        //m_LinkedList.CopyTo(array, arrayIndex);
    }
}