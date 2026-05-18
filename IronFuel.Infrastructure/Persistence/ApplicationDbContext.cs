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
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

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

            modelBuilder.Entity<ApplicationUser>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<ApplicationUser>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<ApplicationUser>()
                .OwnsMany(u => u.RefreshTokens, rt =>
                {
                    rt.WithOwner().HasForeignKey("ApplicationUserId");
                    rt.Property(r => r.Token).IsRequired();
                    rt.HasKey("Id", "ApplicationUserId");
                });

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
            modelBuilder.Entity<ProductVariant>().HasIndex(pv => new { pv.ProductId, pv.FlavourId, pv.WeightG }).IsUnique();
            modelBuilder.Entity<ProductVariant>()
                .Property(a => a.ServingsPerContainer)
                .HasComputedColumnSql("(WeightG / NULLIF(ServingSizeG, 0))", stored: true);
            modelBuilder.Entity<ProductVariant>().Property(p => p.Price).HasPrecision(18, 2);



            modelBuilder.Entity<Flavour>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Flavour>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<Flavour>().HasIndex(f => f.Name).IsUnique();

            modelBuilder.Entity<Cart>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Cart>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<Cart>().HasIndex(c => c.UserId).IsUnique();

            modelBuilder.Entity<CartItem>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<CartItem>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<CartItem>().Property(c => c.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<CartItem>().HasIndex(c => new { c.CartId, c.ProductVariantId }).IsUnique();

            modelBuilder.Entity<Order>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Order>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<Order>().Property(a => a.OrderedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>().Property(a => a.CreatedOn).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<OrderItem>().Property(a => a.LastUpdatedOn).HasDefaultValueSql("NULL");
            modelBuilder.Entity<OrderItem>().Property(o => o.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>().HasIndex(o => new { o.OrderId, o.ProductVariantId }).IsUnique();



            ConfigureUserAudit<Brand>(modelBuilder);
            ConfigureUserAudit<Category>(modelBuilder);
            ConfigureUserAudit<Product>(modelBuilder);
            ConfigureUserAudit<ProductVariant>(modelBuilder);
            ConfigureUserAudit<Flavour>(modelBuilder);
            ConfigureUserAudit<Cart>(modelBuilder);
            ConfigureUserAudit<CartItem>(modelBuilder);
            ConfigureUserAudit<Order>(modelBuilder);
            ConfigureUserAudit<OrderItem>(modelBuilder);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductVariant)
                .WithMany()
                .HasForeignKey(ci => ci.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
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
