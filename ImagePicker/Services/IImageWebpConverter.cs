using ImagePicker.Entities;

namespace ImagePicker.Services
{
    public interface IImageWebpConverter
    {
        Task<Image> ConvertToWebP(Image image);
    }
}
