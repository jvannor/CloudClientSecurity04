using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace CloudClientSecurity04
{
    class Program
    {
        static void Main(string[] args)
        {
            var storageUri = Environment.GetEnvironmentVariable("APP_STORAGE_URI");
            var containerName = Environment.GetEnvironmentVariable("APP_STORAGE_CONTAINER");
            var serviceScope = Environment.GetEnvironmentVariable("APP_SERVICE_SCOPE");
            var serviceUri = Environment.GetEnvironmentVariable("APP_SERVICE_URI");

            var credential = new ChainedTokenCredential(
                new ManagedIdentityCredential(),
                new EnvironmentCredential());

            var functionToken = credential.GetToken(new TokenRequestContext(new string[] { serviceScope }));

            var blobServiceClient = new BlobServiceClient(
                new Uri(storageUri),
                credential);

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient($"{DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")}.jpg");
            blobClient.Upload("test.jpg");

            var functionClient = new HttpClient();
            functionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                functionToken.Token);

            var response = functionClient.GetAsync(serviceUri).Result;
            response.EnsureSuccessStatusCode();
        }
    }
}
