# TinyPng

This is a .NET wrapper around the TinyPng.com image compression service.

* Supports .Net Core and full .Net Framework
* Non-blocking async turtles all the way down
* Byte, stream and file path API's available



## Usage

```csharp
using (var png = new TinyPng("apiKey")) 
{
    await (await png.Compress("pathToFile")).SaveImageToDisk("PathToSave");
}
```
