using ImagePicker.Entities;

namespace ImagePicker.Services
{
    /// <summary>O que o cliente pediu de uma imagem: tamanho, formato e qualidade.</summary>
    public readonly record struct ImageRenderRequest(
        int Width,
        int Height,
        bool PreserveAspect,
        string ContentType,
        int Quality)
    {
        /// <summary>Parte da chave de cache. Tudo que muda os bytes de saida precisa aparecer aqui.</summary>
        public string CacheDiscriminator()
            => $"{Width}x{Height}{(PreserveAspect ? string.Empty : "-fill")}-q{Quality}";
    }

    public interface IImageResizer
    {
        /// <summary>
        /// Decodifica, redimensiona e codifica em uma passada so, sem metadados.
        /// </summary>
        Task<Image> RenderAsync(Image source, ImageRenderRequest request, CancellationToken cancellationToken = default);
    }
}
