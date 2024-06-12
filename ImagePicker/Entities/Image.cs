using FilePicker.Shared;
namespace ImagePicker.Entities
{
    public class Image : Entity
    {
        public Image() : base() 
        {
        }

        public Image(Guid id) : base(id)
        {
        }

        public byte[] File { get; set; } = [];

        public string Extension { get; set; } = String.Empty;
    }
}
