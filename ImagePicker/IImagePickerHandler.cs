using ImagePicker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker
{
    public interface IImagePickerHandler
    {
        Image GetImage(Guid Id, short width, short height, string extension);
    }
}
