using ImagePicker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.Services
{
    internal class ImageDiskPersistanceImpl : IImageDiskPersistance
    {
        private readonly string _cacheDirectory;

        public ImageDiskPersistanceImpl()
        {
            string tempPath = Path.GetTempPath();
            _cacheDirectory = Path.Combine(tempPath, "FilePickerImages");

            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        public void AddCacheImage(Image image, short width, short height)
        {
            string filePath = Path.Combine(_cacheDirectory, $"{image.Id}_{width}x{height}.{image.Extension}");

            File.WriteAllBytes(filePath, image.File);
        }

        public Image GetCachedImage(Guid id, short width, short height, string extension)
        {
            string filePath = Path.Combine(_cacheDirectory, $"{id}_{width}x{height}.{extension}");

            if (File.Exists(filePath))
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                return new Image(id) { File = fileBytes, Extension = extension };
            }
            else
            {
                throw new FileNotFoundException("The cached image could not be found.");
            }
        }
    }
}
