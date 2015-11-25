using System.Threading.Tasks;
using Xunit;

namespace TinyPngApi.Tests
{
    public class TinyPngTests
    {
        const string apiKey = "lolwat";

        [Fact(Skip ="integration")]
        public async Task Test()
        {
            var pngx = new TinyPng(apiKey);

            var result = await (await pngx.Compress("Resources/cat.jpg")).GetImageByteData();


            using (var png = new TinyPng("apiKey"))
            {
                await (await png.Compress("pathToFile")).SaveImageToDisk("PathToSave");
            }

        }
    }
}
