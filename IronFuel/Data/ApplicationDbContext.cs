using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IronFuel.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var AllFKs = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys())
                .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade)
                .ToList();

            foreach (var fk in AllFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<Brand>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Brand>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");

            modelBuilder.Entity<Category>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Category>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");

            modelBuilder.Entity<Product>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Product>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");

            modelBuilder.Entity<ProductVariant>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<ProductVariant>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
        }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
    }

    

}
