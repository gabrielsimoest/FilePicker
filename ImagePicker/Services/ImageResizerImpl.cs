using ImagePicker.Entities;
using SixLabors.ImageSharp.Processing;

namespace ImagePicker.Services
{
    internal class ImageResizerImpl : IImageResizer
    {
        public Entities.Image ResizeImage(Entities.Image image, short width, short height)
        {
            if (image.File == null || image.File.Length == 0)
                throw new ArgumentException("Image file is empty.");

            if(width == 0 && height == 0)
                return image;

            using (var imageStream = new MemoryStream(image.File))
            {
                using (var img = SixLabors.ImageSharp.Image.Load(imageStream))
                {
                    img.Mutate(x => x.Resize(width, height));
                    using (var outputStream = new MemoryStream())
                    {
                        SixLabors.ImageSharp.Formats.IImageEncoder encoder;

                        switch (image.Extension)
                        {
                            case "image/webp":
                                encoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder();
                                break;
                            case "image/png":
                                encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder();
                                break;
                            case "image/jpeg":
                                encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                                break;
                            default:
                                throw new NotSupportedException($"Image format '{image.Extension}' is not supported.");
                        }

                        img.Save(outputStream, encoder);
                        image.File = outputStream.ToArray();
                        image.Extension = image.Extension;
                    }
                }
            }

            return image;
        }
    }
}
