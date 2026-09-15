using ImagePicker.Services;

namespace ImagePicker
{
    /// <summary>Uma variacao ja pronta para ir na resposta HTTP.</summary>
    /// <param name="Content">Bytes codificados.</param>
    /// <param name="ContentType">Media type real do conteudo.</param>
    /// <param name="ETag">Validador forte, ja entre aspas, pronto para o header.</param>
    public sealed record RenderedImage(byte[] Content, string ContentType, string ETag);

    public interface IImagePickerHandler
    {
        /// <summary>
        /// Devolve a variacao pedida, servindo de memoria, de disco ou renderizando na hora.
        /// <c>null</c> quando a imagem nao existe.
        /// </summary>
        Task<RenderedImage?> GetImageAsync(Guid id, ImageRenderRequest request, CancellationToken cancellationToken = default);
    }
}
