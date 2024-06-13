using ImagePicker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.Services
{
    public interface IImageDiskPersistance
    {
        Task AddCacheImage(Image image, short width, short height);
        Task<Image> GetCachedImage(Guid id, short width, short height, string extension);
    }
}
