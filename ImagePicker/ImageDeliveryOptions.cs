namespace ImagePicker
{
    /// <summary>
    /// Ajustes de entrega de imagem. Liga na secao "ImageDelivery" do appsettings.json.
    /// </summary>
    public class ImageDeliveryOptions
    {
        public const string SectionName = "ImageDelivery";

        /// <summary>Qualidade do WebP com perda. 82 mantem SSIM >= 0,92 nos banners e reduz ~9x o tamanho.</summary>
        public int WebpQuality { get; set; } = 82;

        /// <summary>Qualidade do JPEG, quando o formato e pedido explicitamente.</summary>
        public int JpegQuality { get; set; } = 82;

        /// <summary>
        /// Esforco do encoder WebP (0 = rapido/grande, 6 = lento/pequeno).
        /// 4 e o joelho da curva: ~5% menor que 2 por ~30% mais CPU.
        /// </summary>
        public int EncodingMethod { get; set; } = 4;

        /// <summary>
        /// Imagens com ate esse numero de cores distintas (logos, icones, desenhos)
        /// saem em WebP lossless, que nesses casos fica menor E exato.
        /// </summary>
        public int LosslessMaxColors { get; set; } = 256;

        /// <summary>Teto de largura/altura aceito na querystring. Protege CPU e memoria.</summary>
        public int MaxDimension { get; set; } = 4096;

        /// <summary>
        /// Teto de pixels da imagem de origem que o decoder aceita.
        /// Barra "zip bomb" de imagem antes de alocar o bitmap.
        /// </summary>
        public int MaxSourceMegapixels { get; set; } = 64;

        /// <summary>Pasta do cache em disco. Vazio = subpasta "FilePickerImages" no temp do SO.</summary>
        public string? CacheDirectory { get; set; }

        /// <summary>Teto do cache em memoria (camada na frente do disco), em MB. 0 desliga.</summary>
        public int MemoryCacheMegabytes { get; set; } = 256;

        /// <summary>Maior arquivo que entra no cache em memoria, em KB. Acima disso so o disco guarda.</summary>
        public int MemoryCacheMaxItemKilobytes { get; set; } = 512;

        /// <summary>max-age enviado ao navegador/CDN, em dias. A URL e imutavel (id + parametros), entao 1 ano e seguro.</summary>
        public int BrowserCacheDays { get; set; } = 365;

        /// <summary>
        /// Quantas conversoes pesadas rodam ao mesmo tempo. 0 = numero de nucleos.
        /// Evita que uma rajada de cache-miss consuma a CPU toda.
        /// </summary>
        public int MaxConcurrentEncodes { get; set; }

        /// <summary>Qualidades que a querystring "q" pode pedir. Fora da lista, cai no padrao.</summary>
        public int[] AllowedQualities { get; set; } = new[] { 70, 75, 82, 88, 95 };

        public int ResolveConcurrency()
            => MaxConcurrentEncodes > 0 ? MaxConcurrentEncodes : Math.Max(2, Environment.ProcessorCount);
    }
}
