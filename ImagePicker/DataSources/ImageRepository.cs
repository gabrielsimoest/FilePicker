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

        public List<Guid> GetImagesIdsList(short skip, short take)
        {
            return _imageEfCoreDao.ReadImagesIdsList(skip, take);
        }

        public Image GetInformations(Guid id)
        {
            return _imageEfCoreDao.ReadInformations(id);
        }

        public Image SaveImage(Image imageToSave)
        {
            Image image = new Image()
            {
                File = imageToSave.File,
                Extension = imageToSave.Extension,
                Status = true
            };

            _imageEfCoreDao.AddImage(image);

            return image;
        }
    }
}
