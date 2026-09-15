using System.Collections.Concurrent;
using System.Globalization;
using ImagePicker.DataSources;
using ImagePicker.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImagePicker
{
    /// <summary>
    /// Tres camadas: memoria -> disco -> render.
    ///
    /// A versao anterior usava um <c>SemaphoreSlim(1,1)</c> estatico em volta de todo o render,
    /// o que serializava o processo inteiro: uma pagina com 58 imagens frias enfileirava
    /// 58 renders, um de cada vez. Aqui o lock e por chave (so quem pede a MESMA variacao espera)
    /// e a concorrencia global tem teto proprio, para nao tomar a CPU inteira.
    /// </summary>
    public sealed class ImagePickerHandlerImpl : IImagePickerHandler
    {
        private static readonly ConcurrentDictionary<string, Lazy<Task<RenderedImage?>>> InFlight = new();
        private static SemaphoreSlim? _renderSlots;
        private static readonly object RenderSlotsGate = new();

        private readonly IImageDiskPersistance _imageDiskCache;
        private readonly IImageResizer _imageRenderer;
        private readonly IImageRepository _imageRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ImageDeliveryOptions _options;
        private readonly ILogger<ImagePickerHandlerImpl> _logger;

        public ImagePickerHandlerImpl(
            IImageDiskPersistance imageDiskCache,
            IImageResizer imageRenderer,
            IImageRepository imageRepository,
            IMemoryCache memoryCache,
            IOptions<ImageDeliveryOptions> options,
            ILogger<ImagePickerHandlerImpl> logger)
        {
            _imageDiskCache = imageDiskCache;
            _imageRenderer = imageRenderer;
            _imageRepository = imageRepository;
            _memoryCache = memoryCache;
            _options = options.Value;
            _logger = logger;

            EnsureRenderSlots(_options.ResolveConcurrency());
        }

        public async Task<RenderedImage?> GetImageAsync(Guid id, ImageRenderRequest request, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(id, request);

            if (_memoryCache.TryGetValue<RenderedImage>(key, out var fromMemory) && fromMemory is not null)
                return fromMemory;

            var fromDisk = await _imageDiskCache.TryGetAsync(id, request, cancellationToken).ConfigureAwait(false);
            if (fromDisk is not null)
                return Remember(key, new RenderedImage(fromDisk, request.ContentType, BuildETag(key, fromDisk.Length)));

            // Varios clientes pedindo a MESMA variacao fria compartilham um unico render.
            var lazy = InFlight.GetOrAdd(key, cacheKey => new Lazy<Task<RenderedImage?>>(
                () => RenderAndStoreAsync(id, request, cacheKey),
                LazyThreadSafetyMode.ExecutionAndPublication));

            try
            {
                return await lazy.Value.ConfigureAwait(false);
            }
            finally
            {
                InFlight.TryRemove(key, out _);
            }
        }

        private async Task<RenderedImage?> RenderAndStoreAsync(Guid id, ImageRenderRequest request, string key)
        {
            // CancellationToken de proposito ausente: o render alimenta o cache para todo mundo,
            // entao um cliente que desiste no meio nao deve cancelar o trabalho dos outros.
            var slots = _renderSlots!;
            await slots.WaitAsync().ConfigureAwait(false);

            try
            {
                // Outra requisicao pode ter gravado o arquivo enquanto esta esperava pela vaga.
                var fromDisk = await _imageDiskCache.TryGetAsync(id, request).ConfigureAwait(false);
                if (fromDisk is not null)
                    return Remember(key, new RenderedImage(fromDisk, request.ContentType, BuildETag(key, fromDisk.Length)));

                var source = await _imageRepository.GetImage(id).ConfigureAwait(false);
                if (source.File is null || source.File.Length == 0)
                    return null;

                var rendered = await _imageRenderer.RenderAsync(source, request).ConfigureAwait(false);
                if (rendered.File.Length == 0)
                    return null;

                await _imageDiskCache.SaveAsync(id, request, rendered.File).ConfigureAwait(false);

                return Remember(key, new RenderedImage(rendered.File, rendered.Extension, BuildETag(key, rendered.File.Length)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha renderizando a imagem {ImageId} ({Discriminator}).", id, request.CacheDiscriminator());
                return null;
            }
            finally
            {
                slots.Release();
            }
        }

        private RenderedImage Remember(string key, RenderedImage image)
        {
            var maxItemBytes = _options.MemoryCacheMaxItemKilobytes * 1024L;

            if (_options.MemoryCacheMegabytes > 0 && image.Content.Length <= maxItemBytes)
            {
                _memoryCache.Set(key, image, new MemoryCacheEntryOptions
                {
                    Size = image.Content.Length,
                    SlidingExpiration = TimeSpan.FromHours(6)
                });
            }

            return image;
        }

        private static string BuildKey(Guid id, ImageRenderRequest request)
            => string.Create(CultureInfo.InvariantCulture,
                $"{id:N}|{ImageRendererImpl.Normalize(request.ContentType)}|{request.CacheDiscriminator()}");

        /// <summary>
        /// A saida e deterministica para (id, tamanho, formato, qualidade) e um id nunca e reaproveitado,
        /// entao a chave mais o tamanho ja identificam a versao — sem precisar hashear os bytes a cada request.
        /// </summary>
        private static string BuildETag(string key, int length)
            => $"\"{Convert.ToHexString(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes($"{key}|{length}")))[..16].ToLowerInvariant()}\"";

        private static void EnsureRenderSlots(int concurrency)
        {
            if (_renderSlots is not null)
                return;

            lock (RenderSlotsGate)
                _renderSlots ??= new SemaphoreSlim(concurrency, concurrency);
        }
    }
}
