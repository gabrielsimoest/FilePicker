using ImagePicker.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

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

                if (preserveAspect)
                {
                    float aspectRatio = (float)img.Width / img.Height;

                    if (width == 0)
                        width = (short)(height * aspectRatio);
                    else if (height == 0)
                        height = (short)(width / aspectRatio);
                    else
                    {
                        if ((float)width / height > aspectRatio)
                            width = (short)(height * aspectRatio);
                        else
                            height = (short)(width / aspectRatio);
                    }
                }

                img.Mutate(x => x
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = preserveAspect ? ResizeMode.Max : ResizeMode.Crop,
                        Sampler = KnownResamplers.Bicubic
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
                                Quality = 90,
                                Method = WebpEncodingMethod.BestQuality,
                                NearLossless = true
                            };
                            break;
                        case "image/png":
                            encoder = new PngEncoder
                            {
                                CompressionLevel = PngCompressionLevel.BestCompression
                            };
                            break;
                        case "image/jpeg":
                            encoder = new JpegEncoder
                            {
                                Quality = 90
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
