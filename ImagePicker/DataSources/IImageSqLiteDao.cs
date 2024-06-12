using ImagePicker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.DataSources
{
    internal interface IImageSqLiteDao
    {
        Image GetImage(long id);

        void SaveImage(Image image);
    }
}
