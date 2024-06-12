using ImagePicker.Entities;
using Microsoft.EntityFrameworkCore;
namespace ImagePicker.Persistance.Data
{
    internal class ImagesContext : DbContext
    {
        public ImagesContext(DbContextOptions<ImagesContext> options) : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }
    }
}
