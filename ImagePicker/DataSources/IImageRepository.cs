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
        Image GetImage(Guid id);

        void SaveImage(Image image);
    }
}
