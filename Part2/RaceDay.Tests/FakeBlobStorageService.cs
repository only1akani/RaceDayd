using Microsoft.AspNetCore.Http;
using RaceDay.Api.Services;

namespace RaceDay.Tests
{//start of namespace

    //pretends to upload an image so the tests never need a real Azure account
    public class FakeBlobStorageService : IBlobStorageService
    {//start of class

        public Task<string> UploadImageAsync(string containerName, IFormFile file)
        {
            return Task.FromResult($"https://fake-storage.test/{containerName}/{Guid.NewGuid()}.png");
        }

    }//end of class
}//end of namespace