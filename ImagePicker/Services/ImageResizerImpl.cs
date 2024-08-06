using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ImagePicker.Services
{
    internal class ImageResizerImpl : IImageResizer
    {
        public async Task<Entities.Image> ResizeImage(Entities.Image image, short width, short height, bool preserveAspect)
        {
            if (image.File == null || image.File.Length == 0)
                throw new ArgumentException("Image file is empty.");

            if (width == 0 && height == 0)
                return image;

            using (var imageStream = new MemoryStream(image.File))
            {
                var img = await SixLabors.ImageSharp.Image.LoadAsync(imageStream);

                img.Mutate(x => x
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Max,
                        Sampler = KnownResamplers.Lanczos8
                    })
                );

                using (var outputStream = new MemoryStream())
                {
                    IImageEncoder encoder;

                    switch (image.Extension.ToLower())
                    {
                        case "image/webp":
                            encoder = new WebpEncoder
                            {
                                Quality = 80,
                                Method = WebpEncodingMethod.BestQuality,
                                NearLossless = true
                            };
                            break;
                        case "image/png":
                            encoder = new PngEncoder
                            {
                                CompressionLevel = PngCompressionLevel.DefaultCompression
                            };
                            break;
                        case "image/jpeg":
                            encoder = new JpegEncoder
                            {
                                Quality = 80
                            };
                            break;
                        default:
                            throw new NotSupportedException($"Image format '{image.Extension}' is not supported.");
                    }

                    await img.SaveAsync(outputStream, encoder);
                    image.File = outputStream.ToArray();
                }
            }

            return image;
        }
    }
}
