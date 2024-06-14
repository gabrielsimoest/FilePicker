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
        public async Task<IEnumerable<ImagesListResult>> Get(short skip, short take)
        {
            var imagesListResult = new List<ImagesListResult>();
            var Ids = await _imageRepository.GetImagesIdsList(skip, take);

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
        public async Task<InformationResult> GetInfo(Guid id)
        {
            var infos = await _imageRepository.GetInformations(id);

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
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _imageRepository.DeleteImage(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ocorreu um erro interno ao tentar deletar a imagem.", Details = ex.Message });
            }
        }
    }
}
