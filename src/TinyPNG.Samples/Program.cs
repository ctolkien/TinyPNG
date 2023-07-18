using TinyPng;


var tinyPngClient = new TinyPngClient("lolwat");

//var response = await tinyPngClient.Compress(@"./Resources/cat.jpg");
//var x = await tinyPngClient.Compress(@"./Resources/cat.jpg").Resize(100, 100);
//var y = await tinyPngClient.Compress(@"./Resources/cat.jpg").Download().GetImageByteData();

var q = await tinyPngClient.Compress(@"./Resources/cat.jpg").Convert(ConvertImageFormat.Wildcard);

//Console.WriteLine($"Compression Count {x.CompressionCount}");
//Console.WriteLine($"Byte length {y.Length}");

Console.WriteLine($"Converted type = {q.ContentType}");

Console.ReadKey();