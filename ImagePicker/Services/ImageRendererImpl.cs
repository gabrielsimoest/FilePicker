using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StoredImage = ImagePicker.Entities.Image;

namespace ImagePicker.Services
{
    /// <summary>
    /// Decodifica uma vez, redimensiona uma vez, codifica uma vez.
    /// A versao anterior encadeava ImageSharp -> System.Drawing -> ImageSharp,
    /// o que custava 3 decodes/3 encodes e ainda gravava perfil ICC no arquivo final.
    /// </summary>
    internal sealed class ImageRendererImpl : IImageResizer
    {
        private readonly ImageDeliveryOptions _options;

        public ImageRendererImpl(IOptions<ImageDeliveryOptions> options) => _options = options.Value;

        public async Task<StoredImage> RenderAsync(StoredImage source, ImageRenderRequest request, CancellationToken cancellationToken = default)
        {
            if (source.File is null || source.File.Length == 0)
                throw new ArgumentException("Image file is empty.", nameof(source));

            var targetContentType = Normalize(request.ContentType);
            var sourceContentType = Normalize(source.Extension);

            // Sem redimensionar e sem trocar de formato: devolve o original intocado.
            if (request.Width <= 0 && request.Height <= 0 && targetContentType == sourceContentType)
                return source;

            using var input = new MemoryStream(source.File, writable: false);

            var info = await SixLabors.ImageSharp.Image.IdentifyAsync(input, cancellationToken).ConfigureAwait(false);
            var megapixels = (long)info.Width * info.Height / 1_000_000d;
            if (megapixels > _options.MaxSourceMegapixels)
                throw new InvalidOperationException(
                    $"Source image is {megapixels:F1} MP, above the {_options.MaxSourceMegapixels} MP limit.");

            input.Position = 0;

            var (targetWidth, targetHeight) = ResolveTarget(info.Width, info.Height, request);

            // TargetSize deixa o decoder de JPEG escalar durante o decode (DCT scaling),
            // evitando materializar o bitmap em tamanho cheio.
            var decoderOptions = new DecoderOptions
            {
                Configuration = Configuration.Default,
                TargetSize = targetWidth > 0 && targetHeight > 0 ? new Size(targetWidth, targetHeight) : null
            };

            using var image = await SixLabors.ImageSharp.Image
                .LoadAsync<Rgba32>(decoderOptions, input, cancellationToken)
                .ConfigureAwait(false);

            if (targetWidth > 0 && targetHeight > 0 && (image.Width != targetWidth || image.Height != targetHeight))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(targetWidth, targetHeight),
                    Mode = request.PreserveAspect ? ResizeMode.Max : ResizeMode.Stretch,
                    // Lanczos3 entrega praticamente o mesmo resultado do Lanczos8 na reducao,
                    // por uma fracao do custo.
                    Sampler = KnownResamplers.Lanczos3
                }));
            }

            StripMetadata(image);

            using var output = new MemoryStream();
            await image.SaveAsync(output, BuildEncoder(image, targetContentType, request.Quality), cancellationToken)
                .ConfigureAwait(false);

            return new StoredImage(source.Id)
            {
                File = output.ToArray(),
                Extension = targetContentType,
                Status = source.Status
            };
        }

        /// <summary>
        /// Resolve o alvo sem nunca ampliar: ampliar so gasta bytes, nao acrescenta detalhe.
        /// </summary>
        private static (int Width, int Height) ResolveTarget(int sourceWidth, int sourceHeight, ImageRenderRequest request)
        {
            var width = request.Width;
            var height = request.Height;

            if (width <= 0 && height <= 0)
                return (sourceWidth, sourceHeight);

            if (request.PreserveAspect)
            {
                // Dimensao ausente vira "sem limite", que e o comportamento do ResizeMode.Max.
                var boundWidth = width <= 0 ? double.PositiveInfinity : width;
                var boundHeight = height <= 0 ? double.PositiveInfinity : height;

                var scale = Math.Min(boundWidth / sourceWidth, boundHeight / sourceHeight);
                if (scale >= 1d)
                    return (sourceWidth, sourceHeight);

                return (Math.Max(1, (int)Math.Round(sourceWidth * scale)),
                        Math.Max(1, (int)Math.Round(sourceHeight * scale)));
            }

            if (width <= 0) width = sourceWidth;
            if (height <= 0) height = sourceHeight;

            return (Math.Min(width, sourceWidth), Math.Min(height, sourceHeight));
        }

        private static void StripMetadata(Image<Rgba32> image)
        {
            // Um perfil ICC chegava a ser 40% do arquivo de uma marca de 148x38.
            image.Metadata.ExifProfile = null;
            image.Metadata.IccProfile = null;
            image.Metadata.XmpProfile = null;
            image.Metadata.IptcProfile = null;

            foreach (var frame in image.Frames)
            {
                frame.Metadata.ExifProfile = null;
                frame.Metadata.XmpProfile = null;
            }
        }

        private IImageEncoder BuildEncoder(Image<Rgba32> image, string contentType, int quality) => contentType switch
        {
            "image/webp" => BuildWebpEncoder(image, quality),
            "image/png" => new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression,
                SkipMetadata = true
            },
            "image/jpeg" => new JpegEncoder
            {
                Quality = quality,
                Interleaved = false, // progressivo: aparece antes de terminar de baixar
                SkipMetadata = true
            },
            _ => throw new NotSupportedException($"Image format '{contentType}' is not supported.")
        };

        /// <summary>
        /// Escolhe lossless ou com perda pelo conteudo: logo/icone de poucas cores fica menor
        /// E exato em lossless; foto fica muito menor com perda.
        /// O codigo antigo mandava tudo em lossless sem querer, e um banner ia a 1,4 MB.
        /// </summary>
        private WebpEncoder BuildWebpEncoder(Image<Rgba32> image, int quality)
        {
            var method = (WebpEncodingMethod)Math.Clamp(_options.EncodingMethod, 0, 6);

            return HasFewColors(image, _options.LosslessMaxColors)
                ? new WebpEncoder
                {
                    FileFormat = WebpFileFormatType.Lossless,
                    Method = method,
                    SkipMetadata = true
                }
                : new WebpEncoder
                {
                    FileFormat = WebpFileFormatType.Lossy,
                    Quality = quality,
                    Method = method,
                    SkipMetadata = true
                };
        }

        private static bool HasFewColors(Image<Rgba32> image, int limit)
        {
            if (limit <= 0)
                return false;

            var seen = new HashSet<uint>();
            var fewColors = true;

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        seen.Add(pixel.PackedValue);
                        if (seen.Count > limit)
                        {
                            // Foto estoura o limite nas primeiras linhas, entao isso sai cedo.
                            fewColors = false;
                            return;
                        }
                    }
                }
            });

            return fewColors;
        }

        internal static string Normalize(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return "image/webp";

            var value = contentType.Trim().ToLowerInvariant();

            return value switch
            {
                "webp" or "image/webp" => "image/webp",
                "png" or "image/png" => "image/png",
                "jpg" or "jpeg" or "image/jpg" or "image/jpeg" => "image/jpeg",
                _ => value
            };
        }
    }
}
