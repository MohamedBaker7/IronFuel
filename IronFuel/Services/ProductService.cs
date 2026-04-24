using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;

namespace IronFuel.Web.Services
{
    public class ProductService : IProductService
    {
        private static readonly (decimal Min, decimal Max)[] SizeRanges =
        {
            (0.0m, 0.5m),
            (0.5m, 1m),
            (1m, 3m),
            (3m, 6m),
            (6m, 12m)
        };

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public ProductService(ApplicationDbContext context, IMemoryCache cache, IMapper mapper, IImageService imageService)
        {
            _context = context;
            _cache = cache;
            _mapper = mapper;
            _imageService = imageService;
        }

        public ProductsPageViewModel GetProductsPage(int? categoryId, string? categoryName)
        {
            var products = GetProducts(categoryId);
            var productsModel = _mapper.Map<IEnumerable<ProductViewModel>>(products);
            return BuildProductsPageViewModel(productsModel, categoryId, categoryName);
        }

        public IEnumerable<ProductViewModel> GetFilteredProducts(ProductFilterDto filter)
        {
            var products = GetProducts(filter.CategoryId);
            var filteredProducts = ApplyFilters(products, filter);
            return _mapper.Map<IEnumerable<ProductViewModel>>(filteredProducts);
        }

        public async Task<ProductViewModel?> GetProductDetailsAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .ThenInclude(v => v.Flavour)
                .SingleOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product is null)
                return null;

            var viewModel = _mapper.Map<ProductViewModel>(product);
            viewModel.Flavors = product.Variants
                .DistinctBy(v => v.Flavour.Id)
                .OrderBy(v => v.Flavour.Name)
                .Select(v => new SelectListItem { Value = v.Flavour.Id.ToString(), Text = v.Flavour.Name });

            viewModel.Sizes = product.Variants
                .DistinctBy(v => v.WeightG)
                .OrderBy(v => v.WeightG)
                .Select(v => new SelectListItem
                {
                    Value = v.WeightG.ToString(),
                    Text = FormatSizeLabel(v.WeightG, v.ServingsPerContainer)
                });

            return viewModel;
        }

        public async Task<IReadOnlyList<ProductViewModel>> GetManageProductsAsync()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return _mapper.Map<IReadOnlyList<ProductViewModel>>(products);
        }

        public async Task<DataTableResult<ProductViewModel>> GetDataTableProductsAsync(IFormCollection form)
        {
            var draw = form["draw"].FirstOrDefault();
            var skip = int.Parse(form["start"]!);
            var pageSize = int.Parse(form["length"]!);
            var orderColumnIndex = form["order[0][column]"];
            var orderColumnName = form[$"columns[{orderColumnIndex}][name]"];
            var orderDir = form["order[0][dir]"];
            var searchValue = form["search[value]"].ToString();

            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchValue) ||
                    p.Brand!.Name.Contains(searchValue) ||
                    p.Category!.Name.Contains(searchValue));
            }

            var recordsFiltered = await query.CountAsync();
            if (!string.IsNullOrWhiteSpace(orderColumnName))
                query = query.OrderBy($"{orderColumnName} {orderDir}");

            var data = await query.Skip(skip).Take(pageSize).ToListAsync();

            return new DataTableResult<ProductViewModel>
            {
                Draw = draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = _mapper.Map<IReadOnlyList<ProductViewModel>>(data)
            };
        }

        public async Task<ProductEditorViewModel> BuildCreateModelAsync()
        {
            var model = new ProductEditorViewModel();
            await PopulateCatalogOptionsAsync(model);
            return model;
        }

        public async Task<ProductEditorViewModel?> BuildEditModelAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .ThenInclude(v => v.Flavour)
                .Include(p => p.Images)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return null;

            var model = new ProductEditorViewModel
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Description = product.Description,
                Benefits = product.Benefits,
                SuggestedUse = product.SuggestedUse,
                Variants = product.Variants
                    .OrderBy(v => v.Id)
                    .Select(v => new ProductVariantInputViewModel
                    {
                        FlavourId = v.FlavourId,
                        WeightG = v.WeightG,
                        ServingSizeG = v.ServingSizeG,
                        Price = v.Price,
                        Stock = v.Stock
                    })
                    .ToList(),
                ExistingImages = product.Images
                    .OrderBy(i => i.SortOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => new ProductImageEditorViewModel
                    {
                        Id = i.Id,
                        RelativePath = i.RelativePath,
                        SortOrder = i.SortOrder
                    })
                    .ToList()
            };

            model.ExistingImageOrder = string.Join(",", model.ExistingImages.Select(i => i.Id));
            if (model.Variants.Count == 0)
                model.Variants.Add(new ProductVariantInputViewModel());

            await PopulateCatalogOptionsAsync(model);
            return model;
        }

        public async Task<(bool Success, string? ErrorKey, string? ErrorMessage)> CreateAsync(ProductEditorViewModel model)
        {
            var validation = await ValidateEditorAsync(model);
            if (!validation.Success)
                return validation;

            var imageValidation = _imageService.ValidateGalleryImages(model.GalleryImages!, true);
            if (!imageValidation.isValid)
                return (false, nameof(model.GalleryImages), imageValidation.errorMessage);

            var product = new Product
            {
                Name = model.Name.Trim(),
                CategoryId = model.CategoryId,
                BrandId = model.BrandId,
                Description = model.Description.Trim(),
                Benefits = string.IsNullOrWhiteSpace(model.Benefits) ? null : model.Benefits.Trim(),
                SuggestedUse = string.IsNullOrWhiteSpace(model.SuggestedUse) ? null : model.SuggestedUse.Trim(),
                IsDeleted = false
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var variants = model.Variants.Select(v => new ProductVariant
            {
                ProductId = product.Id,
                FlavourId = v.FlavourId,
                WeightG = v.WeightG,
                ServingSizeG = v.ServingSizeG,
                ServingsPerContainer = v.ServingSizeG > 0 ? (int)Math.Round((decimal)v.WeightG / v.ServingSizeG) : 0,
                Price = v.Price,
                Stock = v.Stock,
                IsDeleted = false
            }).ToList();

            if (variants.Count > 0)
                _context.ProductVariants.AddRange(variants);

            var images = await _imageService.SaveProductGalleryImagesAsync(product.Id, model.GalleryImages!);
            if (images.Count > 0)
                _context.ProductImages.AddRange(images);

            await _context.SaveChangesAsync();
            InvalidateProductCache();
            return (true, null, null);
        }

        public async Task<(bool Success, string? ErrorKey, string? ErrorMessage)> UpdateAsync(int id, ProductEditorViewModel model)
        {
            if (id != model.Id)
                return (false, "Id", "Invalid product id.");

            var validation = await ValidateEditorAsync(model);
            if (!validation.Success)
                return validation;

            var imageValidation = _imageService.ValidateGalleryImages(model.GalleryImages!, false);
            if (!imageValidation.isValid)
                return (false, nameof(model.GalleryImages), imageValidation.errorMessage);

            var product = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return (false, "Id", "Product not found.");

            product.Name = model.Name.Trim();
            product.CategoryId = model.CategoryId;
            product.BrandId = model.BrandId;
            product.Description = model.Description.Trim();
            product.Benefits = string.IsNullOrWhiteSpace(model.Benefits) ? null : model.Benefits.Trim();
            product.SuggestedUse = string.IsNullOrWhiteSpace(model.SuggestedUse) ? null : model.SuggestedUse.Trim();

            _context.ProductVariants.RemoveRange(product.Variants);
            var variants = model.Variants.Select(v => new ProductVariant
            {
                ProductId = product.Id,
                FlavourId = v.FlavourId,
                WeightG = v.WeightG,
                ServingSizeG = v.ServingSizeG,
                ServingsPerContainer = v.ServingSizeG > 0 ? (int)Math.Round((decimal)v.WeightG / v.ServingSizeG) : 0,
                Price = v.Price,
                Stock = v.Stock,
                IsDeleted = false
            }).ToList();
            _context.ProductVariants.AddRange(variants);

            var finalImages = await BuildUpdatedGalleryAsync(product, model);
            if (finalImages.Count == 0)
                return (false, nameof(model.GalleryImages), Errors.EmptyGalleryImages);

            _context.ProductImages.RemoveRange(product.Images);
            _context.ProductImages.AddRange(finalImages);

            await _context.SaveChangesAsync();
            InvalidateProductCache();
            return (true, null, null);
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return false;

            product.IsDeleted = !product.IsDeleted;
            product.LastUpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            InvalidateProductCache();
            return true;
        }

        public async Task<(IReadOnlyList<object> Sizes, decimal Price)> GetVariantSelectionDataAsync(int productId, int flavourId, decimal? weightG)
        {
            var sizes = await _context.ProductVariants
                .AsNoTracking()
                .Where(v => v.ProductId == productId && v.FlavourId == flavourId && !v.IsDeleted)
                .Select(v => new { v.WeightG, v.ServingSizeG })
                .Distinct()
                .OrderBy(v => v.WeightG)
                .ToListAsync();

            var selectedWeight = weightG ?? sizes.FirstOrDefault()?.WeightG ?? 0;
            var price = await _context.ProductVariants
                .AsNoTracking()
                .Where(v => v.ProductId == productId && v.FlavourId == flavourId && v.WeightG == selectedWeight && !v.IsDeleted)
                .Select(v => v.Price)
                .FirstOrDefaultAsync();

            return (sizes.Cast<object>().ToList(), price);
        }

        private async Task<(bool Success, string? ErrorKey, string? ErrorMessage)> ValidateEditorAsync(ProductEditorViewModel model)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId && !c.IsDeleted);
            if (!categoryExists)
                return (false, nameof(model.CategoryId), Errors.InvalidCategory);

            var brandExists = await _context.Brands.AnyAsync(b => b.Id == model.BrandId && !b.IsDeleted);
            if (!brandExists)
                return (false, nameof(model.BrandId), Errors.InvalidBrand);

            if (model.Variants.Count == 0)
                return (false, nameof(model.Variants), "Add at least one product variant.");

            var duplicateGroups = model.Variants
                .Select(v => new { v.FlavourId, v.WeightG })
                .GroupBy(v => new { v.FlavourId, v.WeightG })
                .Any(g => g.Key.FlavourId > 0 && g.Count() > 1);

            if (duplicateGroups)
                return (false, nameof(model.Variants), Errors.DuplicatedFlavourAndSize);

            var flavourIds = model.Variants.Select(v => v.FlavourId).Where(id => id > 0).Distinct().ToList();
            var existingFlavourIds = await _context.Flavors
                .AsNoTracking()
                .Where(f => !f.IsDeleted && flavourIds.Contains(f.Id))
                .Select(f => f.Id)
                .ToListAsync();

            if (flavourIds.Except(existingFlavourIds).Any())
                return (false, nameof(model.Variants), Errors.InvalidFlavour);

            return (true, null, null);
        }

        private async Task<List<ProductImage>> BuildUpdatedGalleryAsync(Product product, ProductEditorViewModel model)
        {
            var removedIds = ParseIds(model.RemovedImageIds);
            var imageOrder = ParseIds(model.ExistingImageOrder);

            var existingImages = product.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id).ToList();
            var keptImages = existingImages.Where(i => !removedIds.Contains(i.Id)).ToList();
            var deletedImages = existingImages.Where(i => removedIds.Contains(i.Id)).ToList();

            foreach (var deletedImage in deletedImages)
                await _imageService.DeleteProductImageAsync(deletedImage.RelativePath);

            if (imageOrder.Count > 0)
            {
                var byId = keptImages.ToDictionary(i => i.Id);
                var ordered = new List<ProductImage>();

                foreach (var imageId in imageOrder)
                {
                    if (byId.TryGetValue(imageId, out var image))
                    {
                        ordered.Add(image);
                        byId.Remove(imageId);
                    }
                }

                ordered.AddRange(byId.Values.OrderBy(i => i.SortOrder).ThenBy(i => i.Id));
                keptImages = ordered;
            }

            for (var i = 0; i < keptImages.Count; i++)
                keptImages[i].SortOrder = i;

            var newImages = await _imageService.SaveProductGalleryImagesAsync(product.Id, model.GalleryImages, keptImages.Count);
            keptImages.AddRange(newImages);

            for (var i = 0; i < keptImages.Count; i++)
                keptImages[i].SortOrder = i;

            return keptImages.Select(i => new ProductImage
            {
                ProductId = product.Id,
                RelativePath = i.RelativePath,
                SortOrder = i.SortOrder
            }).ToList();
        }

        private async Task PopulateCatalogOptionsAsync(ProductEditorViewModel model)
        {
            model.CategoryOptions = await _context.Categories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            model.BrandOptions = await _context.Brands
                .AsNoTracking()
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                .ToListAsync();

            model.FlavourOptions = await _context.Flavors
                .AsNoTracking()
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.Name)
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .ToListAsync();
        }

        private List<Product> GetProducts(int? categoryId)
        {
            var cacheKey = categoryId is not null ? $"product_{categoryId}" : "product_all";
            if (_cache.TryGetValue(cacheKey, out List<Product>? products) && products is not null)
                return products;

            products = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .ThenInclude(v => v.Flavour)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && (p.CategoryId == categoryId || categoryId == null))
                .ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, products, cacheOptions);
            return products;
        }

        private static IEnumerable<Product> ApplyFilters(List<Product> products, ProductFilterDto filter)
        {
            if (filter.InStockOnly)
                products = products.Where(p => p.Variants.Any(v => v.Stock > 0)).ToList();

            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice >= filter.MaxPrice)
                filter.MaxPrice = filter.MinPrice + 1;

            if (filter.MinPrice.HasValue)
                products = products.Where(p => p.Variants.Any(v => v.Price >= filter.MinPrice)).ToList();

            if (filter.MaxPrice.HasValue)
                products = products.Where(p => p.Variants.Any(v => v.Price <= filter.MaxPrice)).ToList();

            if (filter.Flavors?.Length > 0)
                products = products.Where(p => p.Variants.Any(v => filter.Flavors.Contains(v.Flavour.Name))).ToList();

            if (filter.Sizes?.Length > 0)
            {
                var parsedRanges = filter.Sizes
                    .Select(s =>
                    {
                        var parts = s.Split('-');
                        if (parts.Length != 2)
                            return null;

                        if (!decimal.TryParse(parts[0], out var min) || !decimal.TryParse(parts[1], out var max))
                            return null;

                        return new { Min = min, Max = max };
                    })
                    .ToList();

                if (!parsedRanges.Any(p => p == null))
                {
                    products = products.Where(p => p.Variants.Any(v =>
                        parsedRanges.Any(r => v.WeightG >= r!.Min * 1000 && v.WeightG < r.Max * 1000)))
                        .ToList();
                }
            }

            return products;
        }

        private ProductsPageViewModel BuildProductsPageViewModel(IEnumerable<ProductViewModel> productsModel, int? categoryId, string? categoryName)
        {
            var products = productsModel.ToList();
            var productVariants = products
                .SelectMany(p => p.Variants, (p, v) => new
                {
                    ProductId = p.Id,
                    FlavourName = v.Flavour.Name,
                    v.Price,
                    v.WeightG
                })
                .ToList();

            var maxPrice = productVariants.Count > 0 ? productVariants.Max(v => v.Price) : 0;
            var flavourFacets = productVariants
                .GroupBy(v => v.FlavourName)
                .Select(g => new ProductFlavourFacetDto
                {
                    Flavour = g.Key,
                    Count = g.Select(v => v.ProductId).Distinct().Count()
                })
                .OrderBy(f => f.Flavour)
                .ToList();

            var sizeFacets = BuildSizeFacets(productVariants.Select(v => (v.ProductId, v.WeightG)).ToList());

            return new ProductsPageViewModel
            {
                CategoryId = categoryId,
                CategoryName = categoryName,
                Products = products,
                MaxPrice = maxPrice,
                Flavours = flavourFacets,
                Sizes = sizeFacets
            };
        }

        private static List<ProductSizeFacetDto> BuildSizeFacets(List<(int ProductId, int Size)> variants)
        {
            return SizeRanges
                .Select(r =>
                {
                    var minG = r.Min * 1000;
                    var maxG = r.Max * 1000;
                    var count = variants
                        .Where(v => v.Size >= minG && v.Size < maxG)
                        .Select(v => v.ProductId)
                        .Distinct()
                        .Count();

                    return new ProductSizeFacetDto
                    {
                        Min = r.Min,
                        Max = r.Max,
                        Label = $"{r.Min} - {r.Max}",
                        Count = count
                    };
                })
                .Where(f => f.Count > 0)
                .ToList();
        }

        private static string FormatSizeLabel(int weightG, int servingsPerContainer)
        {
            var weightLabel = weightG >= 1000
                ? $"{weightG / 1000.0:0.##}kg"
                : $"{weightG}g";

            return $"{weightLabel} ({servingsPerContainer} Servings)";
        }

        private static List<int> ParseIds(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return new List<int>();

            return ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => int.TryParse(value, out var parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        private void InvalidateProductCache() => _cache.Remove("product_all");

        public bool AllowedItem(ProductEditorViewModel model)
        {
            var product = _context.Products.SingleOrDefault(product => product.Name == model.Name && product.BrandId == model.BrandId);

            var isAllowed = product is null || product.Id.Equals(model.Id);

            return isAllowed;
        }
       
    }
}
