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
        List<Guid> GetImagesIdsList(short skip, short take);

        Image GetInformations(Guid id);

        Image GetImage(Guid id);

        Image SaveImage(Image image);
    }
}
