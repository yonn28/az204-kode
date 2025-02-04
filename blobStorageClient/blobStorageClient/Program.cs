using Azure.Identity;
using Azure.Storage.Blobs;
using System;
using System.ComponentModel;
using System.Threading.Tasks;


class Program
{
    static async Task Main(string[] args)
    {
        string endpoint = "https://st02022025-secondary.blob.core.windows.net/";
        var credential = new DefaultAzureCredential();
        BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(endpoint), credential);
        var endpointUri = blobServiceClient.Uri;
        Console.WriteLine("Connnected to "+ endpointUri);

        await ListContainersInAccount(blobServiceClient);

    }

    static async Task ListContainersInAccount(BlobServiceClient blobServiceClient) 
    {
        Console.WriteLine("listing container info ....");
        await foreach (var container in blobServiceClient.GetBlobContainersAsync()) {
            Console.WriteLine($"Container: {container.Name}");
        }
    }
}
