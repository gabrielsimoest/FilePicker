using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.IO;
using System.Threading.Tasks;

namespace ImagePicker.Services
{
    public class ImageWebpConverter : IImageWebpConverter
    {
        public async Task<Entities.Image> ConvertToWebP(Entities.Image image)
        {
            using var inStream = new MemoryStream(image.File);
            var myImage = await Image.LoadAsync(inStream);
            using var outStream = new MemoryStream();

            var encoder = new WebpEncoder
            {
                Quality = 100,
                Method = WebpEncodingMethod.BestQuality,
                NearLossless = true
            };

            await myImage.SaveAsync(outStream, encoder);

            return new Entities.Image(image.Id)
            {
                File = outStream.ToArray(),
                Extension = "image/webp"
            };
        }
    }
}
