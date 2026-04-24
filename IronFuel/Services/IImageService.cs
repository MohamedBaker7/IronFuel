namespace IronFuel.Web.Services
{
    public interface IImageService
    {
        Task<List<ProductImage>> SaveProductGalleryImagesAsync(int productId, IList<IFormFile> files, int startSortOrder = 0);

        (bool isValid, string errorMessage) ValidateGalleryImages(IList<IFormFile> files, bool requireAtLeastOne = true);
        Task DeleteProductImageAsync(string relativePath);
    }
}
