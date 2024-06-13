using ImagePicker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.DataSources
{
    internal interface IImageEfCoreDao
    {
        Task<Image> ReadImage(Guid id);
        Task<Image> ReadInformations(Guid id);

        Task<List<Guid>> ReadImagesIdsList(short skip, short take);

        Task AddImage(Image image);
    }
}
