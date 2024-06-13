using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.Services
{
    public class ImageWebpConverter : IImageWebpConverter
    {
        public Entities.Image ConvertToWebP(Entities.Image image)
        {
            using var inStream = new MemoryStream(image.File);
            using var myImage = Image.Load(inStream);
            using var outStream = new MemoryStream();

            myImage.Save(outStream, new WebpEncoder());

            return new Entities.Image(image.Id)
            {
                File = outStream.ToArray(),
                Extension = "image/webp"
            };
        }
    }
}
