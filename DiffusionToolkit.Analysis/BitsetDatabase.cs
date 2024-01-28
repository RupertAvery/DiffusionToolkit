using Diffusion.Database;
using SparseBitsets;

namespace Diffusion.Analysis;

public class IdScore
{
    public int Id { get; set; }
    public float Score { get; set; }
}

public class BitsetDatabase
{
    private readonly DataStore _dataStore;
    private Vocabulary? _vocabulary;
    private Dictionary<int, SparseBitset> _bitsets;

    public BitsetDatabase(DataStore dataStore)
    {
        _dataStore = dataStore;
        _bitsets = new Dictionary<int, SparseBitset>();
    }

    public void LoadVocabulary()
    {
        throw new NotImplementedException();
    }

    public void SaveDictionary()
    {
        throw new NotImplementedException();
    }

    public void Rebuild()
    {
        _vocabulary = new Vocabulary();

        var prompts = _dataStore.GetImagePrompts().Where(s => !string.IsNullOrEmpty(s.Prompt)).ToList();

        _vocabulary.TokenizeAndAdd(prompts.Select(p => p.Prompt));
    }

    public void ProcessPrompts()
    {
        var prompts = _dataStore.GetImagePrompts().Where(s => !string.IsNullOrEmpty(s.Prompt)).ToList();

        foreach (var imagePrompt in prompts)
        {
            if (imagePrompt.Prompt != null)
            {

                var encodings = _vocabulary.Encode(imagePrompt.Prompt);

                var bitset = new SparseBitset();

                foreach (var encoding in encodings)
                {
                    bitset.Add((uint)encoding);
                }

                bitset.Pack();

                _bitsets.Add(imagePrompt.Id, bitset);
            }
        }
    }

    public void SaveBitsets()
    {
        var bitsets = new List<Bitset>();

        foreach (var bitset in _bitsets)
        {
            var buffer = Serialize(bitset.Value);
            
            bitsets.Add(new Bitset() { Id = bitset.Key, Data = buffer });
        }

        _dataStore.SetBitsets(bitsets);
    }

    public void LoadBitsets()
    {
        var bitsets = _dataStore.GetBitsets();

        _bitsets = new Dictionary<int, SparseBitset>();

        foreach (var bitset in bitsets)
        {
            _bitsets.Add(bitset.Id, Deserialize(bitset.Data));
        }
    }

    public IEnumerable<IdScore> GetSimilarImagesById(int id, float upper, float lower)
    {
        if (_bitsets.TryGetValue(id, out var bitset))
        {
            foreach (var b in _bitsets)
            {
                var jaccard = bitset.GetJaccardIndex(b.Value);

                if (jaccard >= lower && jaccard <= upper)
                {
                    yield return new IdScore { Id = b.Key, Score = jaccard };
                }
            }
        }
    }

    public IEnumerable<IdScore> GetSimilarPrompts(string prompt, float upper, float lower)
    {
        var encodings = _vocabulary.Encode(prompt);

        var bitset = new SparseBitset(encodings);

        bitset.Pack();

        foreach (var b in _bitsets)
        {
            var jaccard = bitset.GetJaccardIndex(b.Value);

            if (jaccard >= lower && jaccard <= upper)
            {
                yield return new IdScore { Id = b.Key, Score = jaccard };
            }
        }
    }

    private SparseBitset Deserialize(byte[] buffer)
    {
        var runs = new List<Run>();
        var offset = 0;

        while (offset < buffer.Length)
        {
            var run = new Run();

            run.Start = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            run.End = BitConverter.ToUInt32(buffer, offset);
            offset += 4;

            var size = run.End - run.Start + 1;

            run.Values = new uint[size];

            for (var i = 0; i < size; i++)
            {
                run.Values[i] = BitConverter.ToUInt32(buffer, offset);
                offset += 4;
            }

            runs.Add(run);
        }

        return new SparseBitset(runs);
    }


    private byte[] Serialize(SparseBitset sparseBitset)
    {
        var runs = sparseBitset.Runs;

        var offset = 0;

        var bufferSize = runs.Sum(r => 8 + r.Values.Length * 4);

        var buffer = new byte[bufferSize];

        foreach (var run in runs)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(run.Start), 0, buffer, offset, 4);
            offset += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(run.End), 0, buffer, offset, 4);
            offset += 4;
            foreach (var runValue in run.Values)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(runValue), 0, buffer, offset, 4);
                offset += 4;
            }
        }

        return buffer;
    }
}