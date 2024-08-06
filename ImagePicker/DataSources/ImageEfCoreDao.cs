using ImagePicker.Entities;
using ImagePicker.Persistance.Data;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Image> ReadImage(Guid id)
        {
            try
            {
                return await _imagesContext.Images.Where(x => x.Id == id).FirstAsync();
            }
            catch
            {
                return new Image(new Guid());
            }
        }

        public async Task AddImage(Image image)
        {
            await _imagesContext.Images.AddAsync(image);
            await _imagesContext.SaveChangesAsync();
        }

        public async Task<List<Guid>> ReadImagesIdsList(short skip, short take)
        {
            try
            {
                return await _imagesContext.Images.Skip(skip).Take(take).Select(x => x.Id).ToListAsync();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public async Task<int> CountImages()
        {
            try
            {
                return await _imagesContext.Images.CountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<Image> ReadInformations(Guid id)
        {
            try
            {
                return await _imagesContext.Images.Where(x => x.Id == id).Select(x => new Image(id)
                {
                    Extension = x.Extension,
                    Status = x.Status
                }).FirstAsync();
            }
            catch
            {
                return new Image(new Guid());
            }
        }

        public async Task DeleteImage(Image image)
        {
            _imagesContext.Images.Remove(image);
            await _imagesContext.SaveChangesAsync();
        }
    }
}
