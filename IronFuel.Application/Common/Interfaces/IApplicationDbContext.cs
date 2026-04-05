using IronFuel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IronFuel.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Flavour> Flavors { get; set; }
        public int SaveChanges();

    }
}
