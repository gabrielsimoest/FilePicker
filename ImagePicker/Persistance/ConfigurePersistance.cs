using ImagePicker.DataSources;
using ImagePicker.Persistance.Data;
using ImagePicker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePicker.Persistance
{
    public static class ConfigurePersistance
    {
        public static void ConfigurePersistenceApp(this IServiceCollection services, IConfiguration configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            AddContexts(services, configuration, serviceLifetime);
            AddServices(services, serviceLifetime);
        }

        private static void AddContexts(IServiceCollection services, IConfiguration configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var connectionString = configuration.GetConnectionString("MainConnection") ?? throw new InvalidOperationException("Connection string 'MainConnection' not found.");
            services.AddDbContext<ImagesContext>(opt => opt.UseSqlServer(connectionString));
            }

        private static void AddServices(IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.AddLogging();

            services.AddDynamic<IImagePickerHandler, ImagePickerHandlerImpl>(serviceLifetime);
            services.AddDynamic<IImageDiskPersistance, ImageDiskPersistanceImpl>(serviceLifetime);
            services.AddDynamic<IImageResizer, ImageResizerImpl>(serviceLifetime);
            services.AddDynamic<IImageEfCoreDao, ImageEfCoreDao>(serviceLifetime);
            services.AddDynamic<IImageRepository, ImageRepository>(serviceLifetime);
        }

        private static void AddDynamic<TInterface, TClass>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TClass : class, TInterface
        where TInterface : class
        {
            services.Add(new ServiceDescriptor(typeof(TInterface), typeof(TClass), lifetime));
        }
    }
}
