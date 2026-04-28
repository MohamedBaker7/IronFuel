using IronFuel.Web.Helpers;
using IronFuel.Web.Services;
using Microsoft.AspNetCore.RateLimiting;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;

namespace IronFuel.Web.Extensions
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddWebServices(this IServiceCollection services, WebApplicationBuilder builder)
        {

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
                options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.User.RequireUniqueEmail = true;
                //opt.Lockout.MaxFailedAccessAttempts = 2;
            });

            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

            builder.Services.AddTransient<IImageService, ImageService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IBrandService, BrandService>();
            builder.Services.AddScoped<IFlavorService, FlavorService>();

            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            builder.Services.AddMvc(option =>
                option.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));


            builder.Services.AddRateLimiter(options =>
                options.AddFixedWindowLimiter("fixed", o =>
                {
                    o.PermitLimit = 10;
                    o.Window = TimeSpan.FromSeconds(10);
                }));


            builder.Services.AddMemoryCache();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // Auto Mapper 
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            builder.Services.AddExpressiveAnnotations();

            return services;
        }
    }
}
