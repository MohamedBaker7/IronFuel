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
        public DbSet<ProductImage> ProductImages { get; set; }

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
            modelBuilder.Entity<Brand>().HasIndex(b => b.Name).IsUnique();

            modelBuilder.Entity<Category>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Category>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

            modelBuilder.Entity<Product>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Product>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<Product>().HasIndex(p => new { p.Name, p.BrandId }).IsUnique();


            modelBuilder.Entity<ProductVariant>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<ProductVariant>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<ProductVariant>().HasIndex(pv => new {pv.ProductId, pv.FlavourId, pv.WeightG}).IsUnique();
            modelBuilder.Entity<ProductVariant>()
                .Property(a => a.ServingsPerContainer)
                .HasComputedColumnSql("(WeightG / NULLIF(ServingSizeG, 0))", stored: true);
            modelBuilder.Entity<ProductVariant>().Property(p => p.Price).HasPrecision(18, 2);



            modelBuilder.Entity<Flavour>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Flavour>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<Flavour>().HasIndex(f => f.Name).IsUnique();



            ConfigureUserAudit<Brand>(modelBuilder);
            ConfigureUserAudit<Category>(modelBuilder);
            ConfigureUserAudit<Product>(modelBuilder);
            ConfigureUserAudit<ProductVariant>(modelBuilder);
            ConfigureUserAudit<Flavour>(modelBuilder);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureUserAudit<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
        {
            modelBuilder.Entity<TEntity>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TEntity>()
                .HasOne(e => e.LastUpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.LastUpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }



}
