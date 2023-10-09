// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using Diffusion.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Diffusion.Civitai;
using Diffusion.Civitai.Models;
using Diffusion.Database;


using var civitai = new CivitaiClient();

var collection = new LiteModelCollection();

var results = await GetPage(1);

collection.Models.AddRange(results.Items);

while (results.Metadata.CurrentPage < results.Metadata.TotalPages)
{
    results = await GetPage(results.Metadata.CurrentPage + 1, results.Metadata.TotalPages);
    collection.Models.AddRange(results.Items);
}

var options = new JsonSerializerOptions()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() }
};

var baseTime = new DateTime(1970, 1, 1, 0, 0, 0);

var mTime = DateTime.Now - baseTime;

collection.Date = mTime.TotalSeconds;

var json = JsonSerializer.Serialize(collection, options);

File.WriteAllText("models.json", json);


async Task<Results<LiteModel>> GetPage(int page, int? total = 0)
{
    Console.WriteLine(total == 0 ? $"Fetching page {page}" : $"Fetching page {page} of {total}");

    return await civitai.GetLiteModelsAsync(new ModelSearchParameters()
    {
        Page = page,
        Limit = 100,
        Types = new List<ModelType>() { ModelType.Checkpoint }
    }, CancellationToken.None);
}


//string path = "D:\\conda\\AUTOMATIC1111\\stable-diffusion-webui\\outputs";

//Scanner s = new Scanner();

//s.Scan(path).ToList();

//var tokens = CSVParser.Parse("Hello there, General Kenobi,\"Pleased, to finally \" \"meet\"\" you!\", I'm sure, it's not me who's pleased");

//foreach (var token in tokens)
//{
//    Console.WriteLine(token);
//}


//string path = "D:\\conda\\AUTOMATIC1111\\stable-diffusion-webui\\models\\Stable-diffusion";



//var scanner = new ModelScanner();
//var files = scanner.Scan(path);

//foreach (var file in files)
//{

//    Console.WriteLine($"{Path.GetFileName(file.Path)}: hash:{file.Hash}, v2:{file.Hashv2}");
//}

//import hashlib
//    m = hashlib.sha256()

//file.seek(0x100000)
//m.update(file.read(0x10000))
//return m.hexdigest()[0:8]