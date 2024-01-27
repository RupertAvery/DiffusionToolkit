using System.Collections;

namespace Diffusion.Analysis;

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

    public IEnumerable<IndexedItem<T>> GetItems()
    {
        return m_LinkedList;
    }

    public void SetItems(IEnumerable<IndexedItem<T>> items)
    {
        Clear();
        foreach (var item in items.OrderBy(item => item.Index))
        {
            var node = m_LinkedList.AddLast(item);
            m_Dictionary.Add(item.Value, node);
        }
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