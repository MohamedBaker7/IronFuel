using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IronFuel.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string[] _AllowedExtension = [".jpg", ".jpeg", ".png", ".webp"];
        private readonly int _AllowedMaximumSize = 2097152;

        public ImageService(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public async Task<List<ProductImage>> SaveProductGalleryImagesAsync(int productId, IList<IFormFile> files, int startSortOrder = 0)
        {
            if (files is null)
                return new List<ProductImage>();

            var images = new List<ProductImage>();
            if (files.Count == 0)
                return images;

            var galleryFolderRelative = Path.Combine("Images", "productImages", productId.ToString());
            var galleryFolderPhysical = Path.Combine(_hostEnvironment.WebRootPath, galleryFolderRelative);
            Directory.CreateDirectory(galleryFolderPhysical);

            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file is null || file.Length == 0)
                    continue;

                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid():N}{fileExtension}";
                var targetPath = Path.Combine(galleryFolderPhysical, fileName);

                await using var stream = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(stream);

                var relativePath = Path.Combine(galleryFolderRelative, fileName).Replace("\\", "/");
                images.Add(new ProductImage
                {
                    ProductId = productId,
                    RelativePath = relativePath,
                    SortOrder = startSortOrder + i
                });
            }

            return images;
        }

        public (bool isValid, string errorMessage) ValidateGalleryImages(IList<IFormFile> files, bool requireAtLeastOne = true)
        {
            if(!requireAtLeastOne && files is null) // validate edit mode
                return (true, string.Empty);

            if (requireAtLeastOne && files.Count == 0) // validate add mode
                return (false, Errors.EmptyImage);

            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file is null || file.Length == 0)
                    return (false, Errors.EmptyImage);


                if (file.Length > _AllowedMaximumSize)
                    return (false, Errors.MaximumSize);


                var extension = Path.GetExtension(file.FileName);
                var isAllowed = _AllowedExtension.Contains(extension, StringComparer.OrdinalIgnoreCase);
                if (!isAllowed)
                    return (false, Errors.AllowedExtension);               
            }

            return (true, string.Empty);
        }

        public Task DeleteProductImageAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.CompletedTask;

            var normalizedPath = relativePath
                .Replace("/", Path.DirectorySeparatorChar.ToString())
                .TrimStart(Path.DirectorySeparatorChar);

            var fullPath = Path.Combine(_hostEnvironment.WebRootPath, normalizedPath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }
    }
}
