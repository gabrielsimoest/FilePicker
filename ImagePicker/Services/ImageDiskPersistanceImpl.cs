using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImagePicker.Services
{
    /// <summary>
    /// Cache em disco, uma pasta por prefixo do id para nao acumular dezenas de milhares
    /// de arquivos num diretorio so.
    /// </summary>
    internal sealed class ImageDiskPersistanceImpl : IImageDiskPersistance
    {
        private readonly string _cacheDirectory;
        private readonly ILogger<ImageDiskPersistanceImpl> _logger;

        public ImageDiskPersistanceImpl(IOptions<ImageDeliveryOptions> options, ILogger<ImageDiskPersistanceImpl> logger)
        {
            _logger = logger;

            var configured = options.Value.CacheDirectory;
            _cacheDirectory = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(Path.GetTempPath(), "FilePickerImages2")
                : configured;

            Directory.CreateDirectory(_cacheDirectory);
        }

        public async Task<byte[]?> TryGetAsync(Guid id, ImageRenderRequest request, CancellationToken cancellationToken = default)
        {
            var path = BuildPath(id, request);

            try
            {
                if (!File.Exists(path))
                    return null;

                var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);

                // Arquivo vazio e resto de escrita interrompida: trata como miss em vez de servir 0 byte.
                return bytes.Length == 0 ? null : bytes;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Falha lendo cache em disco de {ImageId}; seguindo como cache miss.", id);
                return null;
            }
        }

        public async Task SaveAsync(Guid id, ImageRenderRequest request, byte[] content, CancellationToken cancellationToken = default)
        {
            if (content.Length == 0)
                return;

            var path = BuildPath(id, request);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                // Grava num temporario e move: um leitor concorrente nunca ve arquivo pela metade.
                var temporary = $"{path}.{Guid.NewGuid():N}.tmp";
                await File.WriteAllBytesAsync(temporary, content, cancellationToken).ConfigureAwait(false);
                File.Move(temporary, path, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Cache e otimizacao: falhar aqui nao pode derrubar a resposta.
                _logger.LogWarning(ex, "Falha gravando cache em disco de {ImageId}.", id);
            }
        }

        private string BuildPath(Guid id, ImageRenderRequest request)
        {
            var key = id.ToString("N");
            var extension = FileExtensionFor(request.ContentType);

            return Path.Combine(
                _cacheDirectory,
                key[..2],
                $"{key}_{request.CacheDiscriminator()}{extension}");
        }

        private static string FileExtensionFor(string contentType) => ImageRendererImpl.Normalize(contentType) switch
        {
            "image/webp" => ".webp",
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            _ => ".bin"
        };
    }
}
