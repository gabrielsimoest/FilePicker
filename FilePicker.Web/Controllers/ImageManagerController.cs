using FilePicker.Web.Models;
using ImagePicker;
using ImagePicker.DataSources;
using ImagePicker.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace FilePicker.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class ImageManagerController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;

        public ImageManagerController(
            IImageRepository imageRepository
            )
        {
            _imageRepository = imageRepository;
        }

        [HttpGet]
        public IEnumerable<ImagesListResult> Get(short skip, short take)
        {
            var imagesListResult = new List<ImagesListResult>();
            var Ids = _imageRepository.GetImagesIdsList(skip, take);

            var scheme = Request.Scheme;
            var host = Request.Host.Value;

            foreach (var id in Ids)
            {
                var imageResult = new ImagesListResult
                {
                    Id = id,
                    Url = $"{scheme}://{host}/api/Image/{id}?width=64&height=64"
                };
                imagesListResult.Add(imageResult);
            }

            return imagesListResult;
        }

        [HttpGet("{id}")]
        public InformationResult Get(Guid id)
        {
            var infos = _imageRepository.GetInformations(id);

            var scheme = Request.Scheme;
            var host = Request.Host.Value;
            var infoResult = new InformationResult
            {
                Id = id,
                Url = $"{scheme}://{host}/api/Image/{id}?width=64&height=64",
                Status = infos.Status
            };

            return infoResult;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                var image = new Image(new Guid())
                {
                    File = fileBytes,
                    Extension = file.ContentType
                };  

                var savedImage = _imageRepository.SaveImage(image);

                return CreatedAtAction(nameof(Get), new { id = savedImage.Id }, savedImage);
            }
        }

        [HttpPut("{id}")]
        public void Put(Guid id, [FromForm] IFormFile file)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
