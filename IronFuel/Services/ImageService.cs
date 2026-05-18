namespace IronFuel.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string[] _AllowedExtension = [".jpg", ".jpeg", ".png", ".webp"];
        private readonly int _AllowedMaximumSize = 2097152;

        private readonly string[] _AllowedVideoExtensions = [".mp4", ".webm", ".ogg"];
        private const int _AllowedVideoMaximumSize = 52428800; // 50 MB

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

        public (bool isValid, string errorMessage) ValidateProductVideo(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return (true, string.Empty);

            if (file.Length > _AllowedVideoMaximumSize)
                return (false, Errors.VideoMaximumSize);

            var extension = Path.GetExtension(file.FileName);
            var isAllowed = _AllowedVideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
            if (!isAllowed)
                return (false, Errors.InvalidVideoExtension);

            return (true, string.Empty);
        }

        public async Task<string> SaveProductVideoAsync(int productId, IFormFile file)
        {
            var folderRelative = Path.Combine("Videos", "productVideos", productId.ToString());
            var folderPhysical = Path.Combine(_hostEnvironment.WebRootPath, folderRelative);
            if (Directory.Exists(folderPhysical))
                Directory.Delete(folderPhysical, recursive: true);

            Directory.CreateDirectory(folderPhysical);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var targetPath = Path.Combine(folderPhysical, fileName);

            await using var stream = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream);

            var relativePath = Path.Combine(folderRelative, fileName).Replace("\\", "/");
            return "~/" + relativePath;
        }

        public Task DeleteProductVideoByUrlAsync(string? videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
                return Task.CompletedTask;

            var trimmed = videoUrl.Trim();
            if (trimmed.StartsWith("~/", StringComparison.Ordinal))
                trimmed = trimmed[2..];

            var normalizedPath = trimmed
                .Replace("/", Path.DirectorySeparatorChar.ToString())
                .TrimStart(Path.DirectorySeparatorChar);

            var videosRoot = Path.Combine(_hostEnvironment.WebRootPath, "Videos", "productVideos");
            var fullPath = Path.GetFullPath(Path.Combine(_hostEnvironment.WebRootPath, normalizedPath));

            if (!fullPath.StartsWith(Path.GetFullPath(videosRoot), StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);

            return Task.CompletedTask;
        }
    }
}
