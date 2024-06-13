using ImagePicker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.Services
{
    public interface IImageWebpConverter
    {
        Image ConvertToWebP(Image image);
    }
}
