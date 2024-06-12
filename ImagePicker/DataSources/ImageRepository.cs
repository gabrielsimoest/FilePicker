using ImagePicker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.DataSources
{
    internal class ImageRepository : IImageRepository
    {
        public Image GetImage(Guid id)
        {
            throw new NotImplementedException();
        }

        public void SaveImage(Image image)
        {
            throw new NotImplementedException();
        }
    }
}
