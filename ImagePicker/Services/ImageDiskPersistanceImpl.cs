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
            string extension = TreatExtension(image.Extension);

            string filePath = Path.Combine(_cacheDirectory, $"{image.Id}_{width}x{height}{extension}");

            File.WriteAllBytes(filePath, image.File);
        }

        public Image GetCachedImage(Guid id, short width, short height, string extension)
        {
            var extensionToSearch = TreatExtension(extension);

            string filePath = Path.Combine(_cacheDirectory, $"{id}_{width}x{height}{extensionToSearch}");

            if (File.Exists(filePath))
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                return new Image(id) { File = fileBytes, Extension = extension };
            }
            else
            {
                return new Image(new Guid());
            }
        }

        private string TreatExtension(string extension)
        {
            string extensionToReturn = extension switch
            {
                "image/webp" => ".webp",
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                _ => extension
            };

            return extensionToReturn;
        }
    }
}
