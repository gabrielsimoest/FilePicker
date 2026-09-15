namespace ImagePicker.Services
{
    /// <summary>Cache em disco das variacoes ja renderizadas.</summary>
    public interface IImageDiskPersistance
    {
        /// <summary>Bytes da variacao pedida, ou <c>null</c> se ela ainda nao foi renderizada.</summary>
        Task<byte[]?> TryGetAsync(Guid id, ImageRenderRequest request, CancellationToken cancellationToken = default);

        Task SaveAsync(Guid id, ImageRenderRequest request, byte[] content, CancellationToken cancellationToken = default);
    }
}
