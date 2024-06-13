using ImagePicker.Entities;
using ImagePicker.Persistance.Data;
using System.Collections.Generic;

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
            _imagesContext.SaveChanges();
        }

        public List<Guid> ReadImagesIdsList(short skip, short take)
        {
            try
            {
                return _imagesContext.Images.Skip(skip).Take(take).Select(x => x.Id).ToList();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public Image ReadInformations(Guid id)
        {
            try
            {
                return _imagesContext.Images.Where(x => x.Id == id).Select(x => new Image(id)
                {
                    Extension = x.Extension,
                    Status = x.Status
                }).First();
            }
            catch
            {
                return new Image(new Guid());
            }
        }
    }
}
