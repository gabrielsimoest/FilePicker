using ImagePicker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.DataSources
{
    public interface IImageRepository
    {
        Task<List<Guid>> GetImagesIdsList(short skip, short take);

        Task<Image> GetInformations(Guid id);

        Task<Image> GetImage(Guid id);

        Task<Image> SaveImage(Image image);
    }
}
