using ImagePicker;
using Microsoft.AspNetCore.Mvc;

namespace ImageWebPicker.Image
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImagePickerHandler _imagePickerHandler;

        public ImageController(IImagePickerHandler imagePickerHandler)
        {
            _imagePickerHandler = imagePickerHandler;
        }

        [HttpGet]
        public void Get(Guid Id, short width, short height, string extension = "webp")
        {
            _imagePickerHandler.GetImage(Id, width, height, extension);
        }
    }
}
