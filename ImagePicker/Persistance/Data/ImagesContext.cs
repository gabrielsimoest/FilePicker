using ImagePicker.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImagePicker.Persistance.Data
{
    internal class ImagesContext : DbContext
    {
        public ImagesContext(DbContextOptions<ImagesContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Image>()
                .HasKey(i => i.Id);
        }

        public DbSet<Image> Images { get; set; }
    }
}
