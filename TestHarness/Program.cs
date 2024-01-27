// See https://aka.ms/new-console-template for more information

using Diffusion.Database;
using System.Collections;
using Diffusion.Analysis;
using SparseBitsets;
using System.Collections.Generic;
using System;


var dataStore = new DataStore(@"C:\Users\ruper\AppData\Roaming\DiffusionToolkit\diffusion-toolkit.db");

await dataStore.Create(null, null);

Console.WriteLine("Building vocabulary...");

var prompts = dataStore.GetImagePrompts().Where(s => !string.IsNullOrEmpty(s.Prompt)).ToList();

var vocabulary = new Vocabulary();

vocabulary.TokenizeAndAdd(prompts.Select(p => p.Prompt));

vocabulary.GetDictionary();

//Console.WriteLine($"{i} prompts, {c} tokens");

Console.WriteLine($"{vocabulary.Count} unique tokens");

var promptEncodings = new List<PromptBitset>();

foreach (var imagePrompt in prompts)
{
    if (imagePrompt.Prompt != null)
    {

        var encodings = vocabulary.Encode(imagePrompt.Prompt);

        var bitset = new SparseBitset();

        foreach (var encoding in encodings)
        {
            bitset.Add((uint)encoding);
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

//var max = promptEncodings.SelectMany(p => p.Bitset.Runs.Select(r => r.Values.Length + 2)).Max();

//Console.WriteLine(max * 32 / 8);

// Test writing to the database

//var bitsets = new List<Bitset>();

//foreach (var promptEncoding in promptEncodings)
//{
//    var runs = promptEncoding.Bitset.Runs;


//    var offset = 0;

//    var buffer = new byte[1024];


//    foreach (var run in runs)
//    {
//        Buffer.BlockCopy(BitConverter.GetBytes(run.Start), 0, buffer, offset, 4);
//        offset += 4;
//        Buffer.BlockCopy(BitConverter.GetBytes(run.End), 0, buffer, offset, 4);
//        offset += 4;
//        foreach (var runValue in run.Values)
//        {
//            Buffer.BlockCopy(BitConverter.GetBytes(runValue), 0, buffer, offset, 4);
//            offset += 4;
//        }
//    }


//    bitsets.Add(new Bitset() { Id = promptEncoding.Id, Data = buffer });
//}

//Console.WriteLine("Writing to DB...");

//dataStore.SetBitsetBatched(bitsets);

Console.WriteLine("Ready");

Console.ReadLine();

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
