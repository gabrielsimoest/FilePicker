using ImagePicker;
using ImagePicker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace FilePicker.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImagePickerHandler _imagePickerHandler;
        private readonly ImageDeliveryOptions _options;

        public ImageController(IImagePickerHandler imagePickerHandler, IOptions<ImageDeliveryOptions> options)
        {
            _imagePickerHandler = imagePickerHandler;
            _options = options.Value;
        }

        /// <summary>Entrega publica: sempre WebP, que e o formato que a landing consome.</summary>
        [HttpGet("{id:guid}")]
        public Task<IActionResult> Get(Guid id, int width = 0, int height = 0, bool preserveAspect = true, int q = 0, CancellationToken cancellationToken = default)
            => Deliver(id, width, height, preserveAspect, "image/webp", q, cancellationToken);

        /// <summary>Entrega com formato explicito. Protegida: serve para recuperar o original.</summary>
        [Authorize]
        [HttpGet("{id:guid}.{extension}")]
        public Task<IActionResult> Get(Guid id, string extension, int width = 0, int height = 0, bool preserveAspect = true, int q = 0, CancellationToken cancellationToken = default)
            => Deliver(id, width, height, preserveAspect, extension, q, cancellationToken);

        private async Task<IActionResult> Deliver(Guid id, int width, int height, bool preserveAspect, string contentType, int quality, CancellationToken cancellationToken)
        {
            var request = new ImageRenderRequest(
                Clamp(width),
                Clamp(height),
                preserveAspect,
                contentType,
                ResolveQuality(contentType, quality));

            var image = await _imagePickerHandler.GetImageAsync(id, request, cancellationToken);
            if (image is null)
                return NotFound();

            ApplyCachingHeaders(image.ETag);

            // Revalidacao barata: o cliente que ja tem a versao recebe 304 sem corpo.
            if (IsNotModified(image.ETag))
                return StatusCode(StatusCodes.Status304NotModified);

            return File(image.Content, image.ContentType);
        }

        private void ApplyCachingHeaders(string etag)
        {
            var seconds = (long)TimeSpan.FromDays(Math.Max(1, _options.BrowserCacheDays)).TotalSeconds;

            // A URL identifica o conteudo (id + tamanho + formato + qualidade) e um id nunca e
            // reaproveitado, entao a resposta e imutavel. Sem isso o Lighthouse reprova em
            // "Serve static assets with an efficient cache policy" (o valor anterior era 1 dia).
            Response.Headers[HeaderNames.CacheControl] = $"public, max-age={seconds}, immutable";
            Response.Headers[HeaderNames.ETag] = etag;

            // Pragma e Expires eram redundantes com Cache-Control e so gastavam bytes por resposta.
            Response.Headers.Remove(HeaderNames.Pragma);
            Response.Headers.Remove(HeaderNames.Expires);
        }

        private bool IsNotModified(string etag)
        {
            if (!Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNoneMatch))
                return false;

            foreach (var value in ifNoneMatch)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                foreach (var candidate in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (candidate == "*" || candidate == etag)
                        return true;

                    // Um proxy pode devolver o validador enfraquecido (W/"...").
                    if (candidate.StartsWith("W/", StringComparison.Ordinal) && candidate[2..] == etag)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Teto de dimensao: sem ele, <c>?width=30000</c> manda o servidor alocar um bitmap
        /// gigante e ainda cria uma entrada de cache por valor pedido.
        /// </summary>
        private int Clamp(int value)
            => value <= 0 ? 0 : Math.Min(value, _options.MaxDimension);

        private int ResolveQuality(string contentType, int requested)
        {
            var fallback = IsJpeg(contentType) ? _options.JpegQuality : _options.WebpQuality;

            // So valores da lista entram, para nao virar um gerador infinito de variacoes em cache.
            return _options.AllowedQualities.Contains(requested) ? requested : fallback;
        }

        private static bool IsJpeg(string contentType)
            => contentType.Contains("jp", StringComparison.OrdinalIgnoreCase);
    }
}
