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
        Image ReadImage(Guid id);
        Image ReadInformations(Guid id);

        List<Guid> ReadImagesIdsList(short skip, short take);

        void AddImage(Image image);
    }
}
