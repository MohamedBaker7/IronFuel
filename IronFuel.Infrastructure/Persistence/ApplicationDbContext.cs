namespace IronFuel.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Flavour> Flavors { get; set; }

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

            modelBuilder.Entity<Flavour>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Flavour>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
        }
    }



}
