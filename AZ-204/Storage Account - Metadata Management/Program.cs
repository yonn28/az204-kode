using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "DefaultEndpointsProtocol=https;AccountName=st02022025;AccountKey=E4XRz+LAYYXD5HdqY2IqMw9YsQ2bVh+2+5Suno90CecDXlDU0RmcBDGxxkiyghBrqWZnsfv+HYvp+AStWzi/8A==;EndpointSuffix=core.windows.net";
        string containerName = "demo-1";
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var metadata = new Dictionary<string, string>
        {
            { "owner", "admin" },
            { "environment", "production" }
        };

        await containerClient.SetMetadataAsync(metadata);
        Console.WriteLine("Metadata has been set successfully.");

 
        BlobContainerProperties properties = await containerClient.GetPropertiesAsync();
        Console.WriteLine("Container Metadata:");
        foreach (var kvp in properties.Metadata)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }
}


