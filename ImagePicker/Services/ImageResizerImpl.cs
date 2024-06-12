using ImagePicker.Entities;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImagePicker.Services
{
    internal class ImageResizerImpl : IImageResizer
    {
        public Entities.Image ResizeImage(Entities.Image image, short width, short height)
        {
            if (image.File == null || image.File.Length == 0)
            {
                throw new ArgumentException("Image file is empty.");
            }

            using (var inputStream = new MemoryStream(image.File))
            {
                using (var originalImage = System.Drawing.Image.FromStream(inputStream))
                {
                    var resizedBitmap = new Bitmap(width, height);

                    using (var graphics = Graphics.FromImage(resizedBitmap))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;

                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }

                    using (var outputStream = new MemoryStream())
                    {
                        ImageFormat format = GetImageFormat(image.Extension);
                        resizedBitmap.Save(outputStream, format);

                        return new Entities.Image(image.Id)
                        {
                            File = outputStream.ToArray(),
                            Extension = image.Extension
                        };
                    }
                }
            }
        }

        private ImageFormat GetImageFormat(string extension)
        {
            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".gif":
                    return ImageFormat.Gif;
                default:
                    throw new NotSupportedException($"Unsupported image format: {extension}");
            }
        }
    }
}
