using System.Drawing;
using System.Drawing.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace ImagePicker.Services
{
    public class ImageWebpConverter : IImageWebpConverter
    {
        public async Task<Entities.Image> ConvertToWebP(Entities.Image image)
        {
            using var inStream = new MemoryStream(image.File);
            using var originalImage = System.Drawing.Image.FromStream(inStream);

            using var compressedStream = new MemoryStream();

            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 170L);

            var jpgEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
            originalImage.Save(compressedStream, jpgEncoder, encoderParams);

            compressedStream.Seek(0, SeekOrigin.Begin);

            using var imageSharp = await Image.LoadAsync<Rgba32>(compressedStream);

            using var webpStream = new MemoryStream();
            var webpEncoder = new WebpEncoder()
            {
                Quality = 80
            };

            await imageSharp.SaveAsync(webpStream, webpEncoder);

            return new Entities.Image(image.Id)
            {
                File = webpStream.ToArray(),
                Extension = "image/webp"
            };
        }
    }
}
