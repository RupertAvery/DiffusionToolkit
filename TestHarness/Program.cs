// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using Diffusion.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Diffusion.Civitai;
using Diffusion.Civitai.Models;
using Diffusion.Common;
using Diffusion.Database;
using SQLite;

//var dbPath = @"C:\Users\ruper\AppData\Roaming\DiffusionToolkit\Backup-20240420-020535.db";


//var ds = new DataStore(dbPath);
//await ds.Create(null, null);

//DataStore.EnsureFolderExists(new SQLiteConnection(dbPath), "D:\\Backup\\final\\character\\asuka");



//return;

//var migrations = new Migrations(new SQLiteConnection(dbPath), Settings.Instance);
//migrations.RupertAvery20250405_0001_FixFolders();


using var civitai = new CivitaiClient();

var collection = new LiteModelCollection();

var results = await GetNextPage("https://civitai.com/api/v1/models?limit=100&page=1&types=Checkpoint&cursor=3%7C28%7C638698");

collection.Models.AddRange(results.Items);

while (!string.IsNullOrEmpty(results.Metadata.NextPage))
{
    results = await GetNextPage(results.Metadata.NextPage);
    collection.Models.AddRange(results.Items);
}


//while (results.Metadata.CurrentPage < results.Metadata.TotalPages)
//{
//    results = await GetPage(results.Metadata.CurrentPage + 1, results.Metadata.TotalPages);
//    collection.Models.AddRange(results.Items);
//}

//var options = new JsonSerializerOptions()
//{
//    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//    Converters = { new JsonStringEnumConverter() }
//};

//var baseTime = new DateTime(1970, 1, 1, 0, 0, 0);

//var mTime = DateTime.Now - baseTime;

//collection.Date = mTime.TotalSeconds;

//var json = JsonSerializer.Serialize(collection, options);

//File.WriteAllText("models.json", json);


async Task<Results<LiteModel>> GetNextPage(string nextPageUrl)
{
    Console.WriteLine($"Fetching {nextPageUrl}");

    return await civitai.GetLiteModels(nextPageUrl, CancellationToken.None);
}


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