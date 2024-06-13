using ImagePicker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilePicker.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImagePickerHandler _imagePickerHandler;

        public ImageController(IImagePickerHandler imagePickerHandler)
        {
            _imagePickerHandler = imagePickerHandler;
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id, short width = 0, short height = 0, bool preserveAspect = true)
        {
            var image = _imagePickerHandler.GetImage(id, width, height, preserveAspect);
            if (image == null || image.File == null || image.File.Length == 0)
                return NotFound();

            return File(image.File, image.Extension);
        }

        [Authorize]
        [HttpGet("{id}.{extension}")]
        public IActionResult Get(Guid id, string extension, short width = 0, short height = 0, bool preserveAspect = true)
        {
            var image = _imagePickerHandler.GetImage(id, width, height, extension, preserveAspect);
            if (image == null || image.File == null || image.File.Length == 0)
                return NotFound();

            return File(image.File, image.Extension);
        }
    }
}
