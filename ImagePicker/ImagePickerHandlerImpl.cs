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
        private readonly IImageWebpConverter _imageWebpConverter;
        private readonly ILogger<ImagePickerHandlerImpl> _logger;

        public ImagePickerHandlerImpl(
            IImageDiskPersistance imageDiskCache,
            IImageResizer imageResizer,
            IImageRepository imageRepository,
            IImageWebpConverter imageWebpConverter,
            ILogger<ImagePickerHandlerImpl> logger
            )
        {
            _imageDiskCache = imageDiskCache;
            _imageResizer = imageResizer;
            _imageRepository = imageRepository;
            _imageWebpConverter = imageWebpConverter;
            _logger = logger;
        }

        public async Task<Image> GetImage(Guid id, short width, short height, bool preserveAspect)
        {
            var image = new Image(id);
            var extension = "image/webp";

            try
            {
                image = await _imageDiskCache.GetCachedImage(id, width, height, extension);

                if (image.File.Length == 0)
                {
                    image = await _imageRepository.GetImage(id);

                    if (image.File.Length == 0)
                        throw new Exception("Image not found");

                    image = await _imageResizer.ResizeImage(image, width, height, preserveAspect);
                    image = await _imageWebpConverter.ConvertToWebP(image);

                    await _imageDiskCache.AddCacheImage(image, width, height);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return image;
        }

        public async Task<Image> GetImage(Guid id, short width, short height, string extension, bool preserveAspect)
        {
            var image = new Image(id);

            try
            {
                image = await _imageDiskCache.GetCachedImage(id, width, height, extension);

                if (image.File.Length == 0)
                {
                    image = await _imageRepository.GetImage(id);

                    if (image.File.Length == 0)
                        throw new Exception("Image not found");

                    image = await _imageResizer.ResizeImage(image, width, height, preserveAspect);

                    await _imageDiskCache.AddCacheImage(image, width, height);
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
