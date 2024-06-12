using ImagePicker.Entities;

namespace ImagePicker.DataSources
{
    internal class ImageRepository : IImageRepository
    {
        private readonly IImageEfCoreDao _imageEfCoreDao;

        public ImageRepository(IImageEfCoreDao imageEfCoreDao)
        {
            _imageEfCoreDao = imageEfCoreDao;
        }

        public Image GetImage(Guid id)
        {
            return _imageEfCoreDao.ReadImage(id);
        }

        public Image SaveImage(Image imageToSave)
        {
            Image image = new Image()
            {
                File = imageToSave.File,
                Extension = imageToSave.Extension
            };

            _imageEfCoreDao.AddImage(image);

            return image;
        }
    }
}
