using ImagePicker.Entities;

namespace ImagePicker.DataSources
{
    internal interface IImageSqlServerDao
    {
        Image GetImage(long id);

        void SaveImage(Image image);
    }
}
