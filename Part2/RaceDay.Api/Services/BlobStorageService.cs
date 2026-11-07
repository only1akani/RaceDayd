using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{//start of namespace

    //the names of the two containers created in the Azure storage account
    public static class BlobContainers
    {
        public const string EventBanners = "event-banners";
        public const string ProfilePictures = "profile-pictures";
    }

    //checks that an uploaded file is a sensible image before it is sent to Azure
    public static class ImageRules
    {//start of class

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxBytes = 5 * 1024 * 1024;

        //returns an error message, or null if the image is fine
        public static string? Validate(IFormFile? file)
        {//start of method

            if (file == null || file.Length == 0)
            {
                return "Please choose an image file.";
            }

            if (file.Length > MaxBytes)
            {
                return "The image must be 5 MB or smaller.";
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return "Only .jpg, .jpeg, .png and .webp images are allowed.";
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return "The file must be an image.";
            }

            return null;

        }//end of method

    }//end of class

    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(string containerName, IFormFile file);
    }

    //uploads images to Azure Blob Storage and returns the public link
    public class BlobStorageService : IBlobStorageService
    {//start of class

        private readonly IConfiguration _config;

        public BlobStorageService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> UploadImageAsync(string containerName, IFormFile file)
        {//start of method

            //the connection string is only read when a file is uploaded, so the API still starts without Azure
            string? connectionString = _config["AzureStorage:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Azure Storage is not configured.");
            }

            BlobContainerClient container = new BlobServiceClient(connectionString).GetBlobContainerClient(containerName);

            //a random name stops two uploads from overwriting each other
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            BlobClient blob = container.GetBlobClient($"{Guid.NewGuid()}{extension}");

            using (Stream stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blob.Uri.ToString();

        }//end of method

    }//end of class
}//end of namespace