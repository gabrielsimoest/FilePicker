using Microsoft.Extensions.Configuration;
using System.IO;

namespace FilePicker.Web
{
    public class ConnectionStrings
    {
        public IConfiguration Configuration { get; }
        public ConnectionStrings()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            Configuration = builder.Build();
        }

        public ConnectionStrings(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string UserApi
        {
            get
            {
                var conn = Configuration.GetConnectionString("UserApi");
                return conn ?? string.Empty;
            }
        }

        public string PasswordApi
        {
            get
            {
                var conn = Configuration.GetConnectionString("PasswordApi");
                return conn ?? string.Empty;
            }
        }
    }
}

