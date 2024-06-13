using ImagePicker.DataSources;
using ImagePicker.Entities;
using ImagePicker.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker
{
    public class ImagePickerHandlerImpl : IImagePickerHandler
    {
        private readonly IImageDiskPersistance _imageDiskCache;
        private readonly IImageResizer _imageResizer;
        private readonly IImageRepository _imageRepository;
        private readonly ILogger<ImagePickerHandlerImpl> _logger;

        public ImagePickerHandlerImpl(
            IImageDiskPersistance imageDiskCache,
            IImageResizer imageResizer,
            IImageRepository imageRepository,
            ILogger<ImagePickerHandlerImpl> logger
            )
        {
            _imageDiskCache = imageDiskCache;
            _imageResizer = imageResizer;
            _imageRepository = imageRepository;
            _logger = logger;
        }

        public Image GetImage(Guid id, short width, short height, string extension)
        {
            var image = new Image(id);

            try
            {
                image = _imageDiskCache.GetCachedImage(id, width, height, extension);

                if (image.File.Length == 0)
                {
                    image = _imageRepository.GetImage(id);

                    if (image.File.Length == 0)
                        throw new Exception("Image not found");

                    image = _imageResizer.ResizeImage(image, width, height);

                    _imageDiskCache.AddCacheImage(image, width, height);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return image;
        }
    }
}
