// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using Diffusion.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

//Console.WriteLine("Hello, World!");

//string path = "D:\\conda\\AUTOMATIC1111\\stable-diffusion-webui\\outputs";

//Scanner s = new Scanner();

//s.Scan(path).ToList();


string path = "D:\\conda\\AUTOMATIC1111\\stable-diffusion-webui\\models\\Stable-diffusion";



var scanner = new ModelScanner();
var files = scanner.Scan(path);

foreach (var file in files)
{

    Console.WriteLine($"{Path.GetFileName(file.Path)}: hash:{file.Hash}, v2:{file.Hashv2}");
}

//import hashlib
//    m = hashlib.sha256()

//file.seek(0x100000)
//m.update(file.read(0x10000))
//return m.hexdigest()[0:8]