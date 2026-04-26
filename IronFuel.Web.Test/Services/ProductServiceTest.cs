using AutoMapper;
using IronFuel.Application.Common.Interfaces;
using IronFuel.Application.DTO.Products;
using IronFuel.Domain.Entities;
using IronFuel.Infrastructure.Persistence;
using IronFuel.Web.Core.Mapping;
using IronFuel.Web.Core.ViewModels;
using IronFuel.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace IronFuel.Web.Test.Services
{
    public class ProductServiceTests
    {
        private readonly IApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        private readonly Mock<IImageService> _imageServiceMock;

        private readonly ProductService _sut;

        public ProductServiceTests()
        {

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _cache = new MemoryCache(new MemoryCacheOptions());

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            }, NullLoggerFactory.Instance);

            _mapper = mapperConfig.CreateMapper();

            _imageServiceMock = new Mock<IImageService>();

            _sut = new ProductService(_context, _cache, _mapper, _imageServiceMock.Object);
        }


        private Category SeedCategory(int id = 1, string name = "Protein", bool isDeleted = false)
        {
            var category = new Category { Id = id, Name = name, IsDeleted = isDeleted };
            _context.Categories.Add(category);
            _context.SaveChanges();
            return category;
        }

        private Brand SeedBrand(int id = 1, string name = "Optimum Nutrition", bool isDeleted = false)
        {
            var brand = new Brand { Id = id, Name = name, IsDeleted = isDeleted };
            _context.Brands.Add(brand);
            _context.SaveChanges();
            return brand;
        }

        private Flavour SeedFlavour(int id = 1, string name = "Chocolate", bool isDeleted = false)
        {
            var flavour = new Flavour { Id = id, Name = name, IsDeleted = isDeleted };
            _context.Flavors.Add(flavour);
            _context.SaveChanges();
            return flavour;
        }

        private Product SeedProduct(int id = 1, string name = "Whey Protein", int categoryId = 1,
            int brandId = 1, bool isDeleted = false, string Description = "Description")
        {
            var product = new Product
            {
                Id = id,
                Name = name,
                CategoryId = categoryId,
                BrandId = brandId,
                Description = "Test description",
                IsDeleted = isDeleted
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            return product;
        }

        private ProductVariant SeedVariant(int productId, int flavourId, int weightG = 1000,
            decimal price = 29.99m, int stock = 10, bool isDeleted = false)
        {
            var variant = new ProductVariant
            {
                ProductId = productId,
                FlavourId = flavourId,
                WeightG = weightG,
                Price = price,
                Stock = stock,
                ServingSizeG = 30,
                ServingsPerContainer = 33,
                IsDeleted = isDeleted
            };
            _context.ProductVariants.Add(variant);
            _context.SaveChanges();
            return variant;
        }


        [Fact]
        public void AllowedItem_WhenNoProductWithSameName_ReturnsTrue()
        {
            // Arrange — empty DB
            var model = new ProductEditorViewModel { Name = "Whey Protein", BrandId = 1 };

            // Act
            var result = _sut.AllowedItem(model);

            // Assert
            Assert.True(result);
        }
        [Fact]
        public void AllowedItem_WhenSameNameAndBrandExistWithDifferentId_ReturnsFalse()
        {
            // Arrange
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 99, name: "Whey Protein", brandId: 1);

            var model = new ProductEditorViewModel
            {
                Id = 1,
                Name = "Whey Protein",
                BrandId = 1
            };

            // Act
            var result = _sut.AllowedItem(model);

            // Assert
            Assert.False(result);
        }
        [Fact]
        public void AllowedItem_WhenEditingSameProduct_ReturnsTrue()
        {
            // Arrange
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 5, name: "Whey Protein", brandId: 1);

            var model = new ProductEditorViewModel
            {
                Id = 5, 
                Name = "Whey Protein",
                BrandId = 1
            };

            // Act
            var result = _sut.AllowedItem(model);

            // Assert
            Assert.True(result);
        }
        [Fact]
        public void AllowedItem_WhenSameNameDifferentBrand_ReturnsTrue()
        {
            // Arrange
            SeedCategory();
            SeedBrand(id: 1);
            SeedBrand(id: 2, name: "MyProtein");
            SeedProduct(id: 1, name: "Whey Protein", brandId: 1);

            var model = new ProductEditorViewModel
            {
                Id = 0,
                Name = "Whey Protein",
                BrandId = 2,
                
            };

            // Act
            var result = _sut.AllowedItem(model);

            // Assert
            Assert.True(result);
        }



        [Fact]
        public async Task ToggleStatusAsync_WhenProductIsActive_SetsIsDeletedTrue()
        {
            // Arrange
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 1, isDeleted: false);

            // Act
            var result = await _sut.ToggleStatusAsync(1);

            // Assert
            Assert.True(result);
            var product = await _context.Products.FindAsync(1);
            Assert.True(product!.IsDeleted);
        }

        [Fact]
        public async Task ToggleStatusAsync_WhenProductIsDeleted_SetsIsDeletedFalse()
        {
            // Arrange
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 1, isDeleted: true);

            // Act
            var result = await _sut.ToggleStatusAsync(1);

            // Assert
            Assert.True(result);
            var product = await _context.Products.FindAsync(1);
            Assert.False(product!.IsDeleted);
        }
        [Fact]
        public async Task ToggleStatusAsync_WhenProductNotFound_ReturnsFalse()
        {
            var result = await _sut.ToggleStatusAsync(999);

            Assert.False(result);
        }



        [Fact]
        public async Task GetProductDetailsAsync_WhenProductNotFound_ReturnsNull()
        {
            var result = await _sut.GetProductDetailsAsync(999);

            Assert.Null(result);
        }
        [Fact]
        public async Task GetProductDetailsAsync_WhenProductIsDeleted_ReturnsNull()
        {
            // Arrange — soft-deleted product
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 1, isDeleted: true);

            // Act
            var result = await _sut.GetProductDetailsAsync(1);

            // Assert — deleted products are invisible
            Assert.Null(result);
        }
        [Fact]
        public async Task GetProductDetailsAsync_WhenProductExists_ReturnsViewModel()
        {
            // Arrange
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 1, name: "Whey Protein", isDeleted: false);

            // Act
            var result = await _sut.GetProductDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Whey Protein", result.Name);
        }



        [Fact]
        public async Task GetManageProductsAsync_ReturnsAllProducts_IncludingDeleted()
        {
            // Arrange — 2 active + 1 deleted
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 1, name: "Whey Protein");
            SeedProduct(id: 2, name: "Creatine");
            SeedProduct(id: 3, name: "BCAA", isDeleted: true);

            // Act
            var result = await _sut.GetManageProductsAsync();

            // Assert — manage view shows all (including deleted)
            Assert.Equal(3, result.Count);
        }
        [Fact]
        public async Task GetManageProductsAsync_WhenEmpty_ReturnsEmptyList()
        {
            var result = await _sut.GetManageProductsAsync();

            Assert.Empty(result);
        }
        [Fact]
        public async Task GetManageProductsAsync_ReturnsProductsOrderedByName()
        {
            // Arrange
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 1, name: "Zinc");
            SeedProduct(id: 2, name: "Creatine");
            SeedProduct(id: 3, name: "Whey Protein");

            // Act
            var result = await _sut.GetManageProductsAsync();

            // Assert — should be alphabetical
            var names = result.Select(p => p.Name).ToList();
            Assert.Equal(names.OrderBy(n => n).ToList(), names);
        }



        [Fact]
        public void GetFilteredProducts_WithInStockOnly_ExcludesOutOfStockProducts()
        {
            // Arrange
            SeedCategory(id: 1);
            SeedBrand(id: 1);
            SeedFlavour(id: 1);
            SeedProduct(id: 1, name: "In Stock Product");
            SeedProduct(id: 2, name: "Out of Stock Product");
            SeedVariant(productId: 1, flavourId: 1, stock: 10);  // in stock
            SeedVariant(productId: 2, flavourId: 1, stock: 0);   // out of stock

            var filter = new ProductFilterDto { InStockOnly = true };

            // Act
            var result = _sut.GetFilteredProducts(filter);

            // Assert
            Assert.Single(result);
            Assert.Equal("In Stock Product", result.First().Name);
        }
        [Fact]
        public void GetFilteredProducts_WithMinPrice_ExcludesCheaperProducts()
        {
            // Arrange
            SeedCategory(id: 1);
            SeedBrand(id: 1);
            SeedFlavour(id: 1);
            SeedProduct(id: 1, name: "Cheap Product");
            SeedProduct(id: 2, name: "Expensive Product");
            SeedVariant(productId: 1, flavourId: 1, price: 10m);
            SeedVariant(productId: 2, flavourId: 1, price: 50m);

            var filter = new ProductFilterDto { MinPrice = 30m };

            // Act
            var result = _sut.GetFilteredProducts(filter);

            // Assert
            Assert.Single(result);
            Assert.Equal("Expensive Product", result.First().Name);
        }
        [Fact]
        public void GetFilteredProducts_WithMaxPrice_ExcludesExpensiveProducts()
        {
            // Arrange
            SeedCategory(id: 1);
            SeedBrand(id: 1);
            SeedFlavour(id: 1);
            SeedProduct(id: 1, name: "Cheap Product");
            SeedProduct(id: 2, name: "Expensive Product");
            SeedVariant(productId: 1, flavourId: 1, price: 10m);
            SeedVariant(productId: 2, flavourId: 1, price: 50m);

            var filter = new ProductFilterDto { MaxPrice = 30m };

            // Act
            var result = _sut.GetFilteredProducts(filter);

            // Assert
            Assert.Single(result);
            Assert.Equal("Cheap Product", result.First().Name);
        }
        [Fact]
        public void GetFilteredProducts_WithFlavourFilter_ReturnsMatchingProducts()
        {
            // Arrange
            SeedCategory(id: 1);
            SeedBrand(id: 1);
            SeedFlavour(id: 1, name: "Chocolate");
            SeedFlavour(id: 2, name: "Vanilla");
            SeedProduct(id: 1, name: "Chocolate Whey");
            SeedProduct(id: 2, name: "Vanilla Whey");
            SeedVariant(productId: 1, flavourId: 1);
            SeedVariant(productId: 2, flavourId: 2);

            var filter = new ProductFilterDto { Flavors = new[] { "Chocolate" } };

            // Act
            var result = _sut.GetFilteredProducts(filter);

            // Assert
            Assert.Single(result);
            Assert.Equal("Chocolate Whey", result.First().Name);
        }
        [Fact]
        public void GetFilteredProducts_WithNoFilters_ReturnsAllActiveProducts()
        {
            // Arrange
            SeedCategory(id: 1);
            SeedBrand(id: 1);
            SeedFlavour(id: 1);
            SeedProduct(id: 1, name: "Whey Protein");
            SeedProduct(id: 2, name: "Creatine");
            SeedProduct(id: 3, name: "Deleted Product", isDeleted: true);

            var filter = new ProductFilterDto();

            // Act
            var result = _sut.GetFilteredProducts(filter);

            // Assert — deleted products excluded
            Assert.Equal(2, result.Count());
        }



        [Fact]
        public async Task BuildEditModelAsync_WhenProductNotFound_ReturnsNull()
        {
            var result = await _sut.BuildEditModelAsync(999);

            Assert.Null(result);
        }
        [Fact]
        public async Task BuildEditModelAsync_WhenProductExists_ReturnsPopulatedModel()
        {
            // Arrange
            SeedCategory(id: 1);
            SeedBrand(id: 1);
            SeedFlavour(id: 1);
            SeedProduct(id: 1, name: "Whey Protein", categoryId: 1, brandId: 1);
            SeedVariant(productId: 1, flavourId: 1);

            // Act
            var result = await _sut.BuildEditModelAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Whey Protein", result.Name);
            Assert.NotEmpty(result.Variants);
        }
        [Fact]
        public async Task BuildEditModelAsync_WhenProductHasNoVariants_AddsEmptyVariant()
        {
            // Arrange — product with no variants
            SeedCategory(id: 1);
            SeedBrand(id: 1);
            SeedProduct(id: 1, name: "Whey Protein");

            // Act
            var result = await _sut.BuildEditModelAsync(1);

            // Assert — should add one empty variant so form isn't blank
            Assert.NotNull(result);
            Assert.Single(result.Variants);
        }



        [Fact]
        public async Task GetVariantSelectionDataAsync_WhenVariantsExist_ReturnsSizesAndPrice()
        {
            // Arrange
            SeedCategory();
            SeedBrand();
            SeedFlavour(id: 1);
            SeedProduct(id: 1);
            SeedVariant(productId: 1, flavourId: 1, weightG: 1000, price: 29.99m);

            // Act
            var (sizes, price) = await _sut.GetVariantSelectionDataAsync(1, 1, null);

            // Assert
            Assert.NotEmpty(sizes);
            Assert.Equal(29.99m, price);
        }
        [Fact]
        public async Task GetVariantSelectionDataAsync_WhenNoVariants_ReturnsEmptyAndZeroPrice()
        {
            // Arrange — product exists but no variants for this flavour
            SeedCategory();
            SeedBrand();
            SeedProduct(id: 1);

            // Act
            var (sizes, price) = await _sut.GetVariantSelectionDataAsync(1, 99, null);

            // Assert
            Assert.Empty(sizes);
            Assert.Equal(0m, price);
        }
        [Fact]
        public async Task GetVariantSelectionDataAsync_WithSpecificWeight_ReturnsCorrectPrice()
        {
            // Arrange — two variants with different weights and prices
            SeedCategory();
            SeedBrand();
            SeedFlavour(id: 1);
            SeedProduct(id: 1);
            SeedVariant(productId: 1, flavourId: 1, weightG: 1000, price: 29.99m);
            SeedVariant(productId: 1, flavourId: 1, weightG: 2000, price: 49.99m);

            // Act — ask specifically for the 2kg variant
            var (sizes, price) = await _sut.GetVariantSelectionDataAsync(1, 1, 2000);

            // Assert — should return the 2kg price, not the 1kg one
            Assert.Equal(49.99m, price);
        }
        [Fact]
        public async Task GetVariantSelectionDataAsync_WhenWeightIsNull_ReturnsFirstVariantPrice()
        {
            // Arrange — two variants, weightG null means "pick the first one"
            SeedCategory();
            SeedBrand();
            SeedFlavour(id: 1);
            SeedProduct(id: 1);
            SeedVariant(productId: 1, flavourId: 1, weightG: 500, price: 15.99m);
            SeedVariant(productId: 1, flavourId: 1, weightG: 1000, price: 29.99m);

            // Act — no weight specified → defaults to smallest weight (500g)
            var (sizes, price) = await _sut.GetVariantSelectionDataAsync(1, 1, null);

            // Assert
            Assert.Equal(2, sizes.Count);       // both sizes returned
            Assert.Equal(15.99m, price);        // price of smallest weight
        }
        [Fact]
        public async Task GetVariantSelectionDataAsync_ExcludesDeletedVariants()
        {
            // Arrange — one active, one deleted variant
            SeedCategory();
            SeedBrand();
            SeedFlavour(id: 1);
            SeedProduct(id: 1);
            SeedVariant(productId: 1, flavourId: 1, weightG: 1000, price: 29.99m, isDeleted: false);
            SeedVariant(productId: 1, flavourId: 1, weightG: 2000, price: 49.99m, isDeleted: true);

            // Act
            var (sizes, price) = await _sut.GetVariantSelectionDataAsync(1, 1, null);

            // Assert — deleted variant should not appear
            Assert.Single(sizes);
            Assert.Equal(29.99m, price);
        }
    }
}
