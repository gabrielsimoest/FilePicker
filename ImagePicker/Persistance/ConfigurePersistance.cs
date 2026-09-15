using ImagePicker.DataSources;
using ImagePicker.Persistance.Data;
using ImagePicker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;

namespace ImagePicker.Persistance
{
    public static class ConfigurePersistance
    {
        public static void ConfigurePersistenceApp(this IServiceCollection services, IConfiguration configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            AddOptions(services, configuration);
            AddContexts(services, configuration, serviceLifetime);
            AddServices(services, serviceLifetime);
        }

        private static void AddOptions(IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection(ImageDeliveryOptions.SectionName);
            services.Configure<ImageDeliveryOptions>(section);

            var options = section.Get<ImageDeliveryOptions>() ?? new ImageDeliveryOptions();

            // Teto no pool de buffers do ImageSharp. Sem isso, uma rajada de imagens grandes
            // pode segurar centenas de MB no processo.
            Configuration.Default.MemoryAllocator = MemoryAllocator.Create(new MemoryAllocatorOptions
            {
                MaximumPoolSizeMegabytes = Math.Max(32, options.MemoryCacheMegabytes / 2)
            });

            services.AddMemoryCache(cache =>
            {
                // SizeLimit em bytes: cada entrada e registrada com Size = tamanho do arquivo.
                cache.SizeLimit = Math.Max(1, options.MemoryCacheMegabytes) * 1024L * 1024L;
                cache.CompactionPercentage = 0.25;
            });
        }

        private static void AddContexts(IServiceCollection services, IConfiguration configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var connectionString = configuration.GetConnectionString("MainConnection") ?? throw new InvalidOperationException("Connection string 'MainConnection' not found.");

            services.AddDbContext<ImagesContext>(opt => opt
                // As leituras de imagem sao somente-leitura: sem change tracking a EF nao guarda
                // uma segunda copia do byte[] do original (que chega a 3 MB).
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(connectionString));
        }

        private static void AddServices(IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.AddLogging();

            services.AddDynamic<IImagePickerHandler, ImagePickerHandlerImpl>(serviceLifetime);
            services.AddDynamic<IImageResizer, ImageRendererImpl>(serviceLifetime);
            services.AddDynamic<IImageEfCoreDao, ImageEfCoreDao>(serviceLifetime);
            services.AddDynamic<IImageRepository, ImageRepository>(serviceLifetime);

            // O cache em disco nao guarda estado por requisicao e cria o diretorio no construtor:
            // singleton evita refazer isso a cada chamada.
            services.AddSingleton<IImageDiskPersistance, ImageDiskPersistanceImpl>();
        }

        private static void AddDynamic<TInterface, TClass>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TClass : class, TInterface
        where TInterface : class
        {
            services.Add(new ServiceDescriptor(typeof(TInterface), typeof(TClass), lifetime));
        }

    }
}
