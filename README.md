# TinyPng

This is a .NET wrapper around the TinyPng.com image compression service.

* Supports .Net Core and full .Net Framework
* Non-blocking async turtles all the way down
* `Byte[]`, `Stream` and `File` API's available

## Installation

Install via Nuget

```
    Install-Package TinyPNG
```

## Compressing Image

```csharp
using (var png = new TinyPngClient("yourSecretApiKey")) 
{
    var compressResult = await png.Compress("pathToFile or byte array or stream)");

    //get the image data as a byte array
    var bytes = await compressResult.GetImageByteData();

    //get a stream instead
    var stream  = await compressResult.GetImageStreamData()

    //or just save to disk
    await compressResult.SaveImageToDisk("pathToSaveImage");

}
```

Further details about the result of the compression are also available on the `Input` and `Output` properties. Some examples:
```csharp

    //old size
    compressResult.Input.Size;
    
    //new size
    compressResult.Output.Size;

    //URL of the compressed Image
    compressResult.Output.Url;

```

## Resizing Images

```csharp
using (var png = new TinyPngClient("yourSecretApiKey")) 
{
    var compressResult = await png.Compress("pathToFile or byte array or stream)");
    
    var resizedImage = await png.Resize(compressResult, width, height, ResizeType);

    await resizedImage.SaveImageToDisk("pathToSaveImage");
}

```

### Resize Operations

There are certain combinations when specifying resize options which aren't compatible with
TinyPNG. We also include strongly type resize operations, 
depending on the type of resize you want to do. 

```csharp
using (var png = new TinyPngClient("yourSecretApiKey")) 
{
    var compressResult = await png.Compress("pathToFile or byte array or stream)");
    
    await png.Resize(compressResult, new ScaleWidthResizeOperation(width));
    await png.Resize(compressResult, new ScaleHeightResizeOperation(width));
    await png.Resize(compressResult, new FitResizeOperation(width, height));
    await png.Resize(compressResult, new CoverResizeOperation(width, height));
}

```

The same `Byte[]`, `Stream` and `File` path API's are available from the result of the `Resize()` method.