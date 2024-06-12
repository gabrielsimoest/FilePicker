using ImagePicker.Entities;
using ImagePicker.Persistance.Data;

namespace ImagePicker.DataSources
{
    internal class ImageEfCoreDao : IImageEfCoreDao
    {
        private readonly  ImagesContext _imagesContext;

        public ImageEfCoreDao(ImagesContext imagesContext) 
        {
            _imagesContext = imagesContext;
        }

        public Image ReadImage(Guid id)
        {
            try
            {
                return _imagesContext.Images.Where(x => x.Id == id).First();
            }
            catch
            {
                return new Image(new Guid());
            }
        }

        public void AddImage(Image image)
        {
            _imagesContext.Images.Add(image);
        }
    }
}
