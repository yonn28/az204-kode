using System;
using System.IO;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;


namespace func4
{
    public class WebTableTrigger
    {
        private readonly ILogger<WebTableTrigger> _logger;

        public WebTableTrigger(ILogger<WebTableTrigger> logger)
        {
            _logger = logger;
        }

        [Function("WebTableTrigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Processing HTTP request to write to Azure Table Storage.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Request Body: {requestBody}");

            dynamic data;
            try
            {
                data = JsonConvert.DeserializeObject(requestBody);
            }
            catch (JsonException jsonEx)
            {
                log.LogError($"JSON Deserialization Error: {jsonEx.Message}");
                return new BadRequestObjectResult($"Invalid JSON format: {jsonEx.Message}");
            }

            log.LogInformation($"Deserialized Data: {data}");

            string partitionKey = data?.partitionKey ?? "defaultPartition";
            string rowKey = data?.rowKey ?? Guid.NewGuid().ToString();

            var tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "UserDetails");

            log.LogInformation($"webstorage : {Environment.GetEnvironmentVariable("AzureWebJobsStorage")}");

            try
            {
                var entity = new TableEntity(partitionKey, rowKey)
                {
                    { "Name", (string)data.name },
                    { "Email", (string)data.email }
                };

                log.LogInformation($"Entity to be added: PartitionKey={partitionKey}, RowKey={rowKey}, Name={data?.name}, Email={data?.email}");

                await tableClient.AddEntityAsync(entity);

                log.LogInformation("Data written to table storage successfully.");
                return new OkObjectResult("Data written to table storage");
            }
            catch (Exception ex)
            {
                log.LogError($"Error writing to table storage: {ex.Message}");
                return new BadRequestObjectResult($"Error writing to table storage: {ex.Message}");
            }
        }
    }
}
