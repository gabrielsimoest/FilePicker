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

        public async Task<Image> GetImage(Guid id)
        {
            return await _imageEfCoreDao.ReadImage(id);
        }

        public async Task<List<Guid>> GetImagesIdsList(short skip, short take)
        {
            return await _imageEfCoreDao.ReadImagesIdsList(skip, take);
        }

        public async Task<Image> GetInformations(Guid id)
        {
            return await _imageEfCoreDao.ReadInformations(id);
        }

        public async Task<Image> SaveImage(Image imageToSave)
        {
            Image image = new Image()
            {
                File = imageToSave.File,
                Extension = imageToSave.Extension,
                Status = true
            };

            await _imageEfCoreDao.AddImage(image);

            return image;
        }
    }
}
