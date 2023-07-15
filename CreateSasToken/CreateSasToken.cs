using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure;

namespace CreateSasToken
{
    public static class CreateSasToken
    {
        [FunctionName("CreateSasToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string blobName = req.Query["name"];
            var accountKey = Environment.GetEnvironmentVariable("AccountKey");
            var contaunerName = Environment.GetEnvironmentVariable("BlobName");

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(accountKey));

            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(contaunerName);
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference(blobName);

            var sasContraints = new SharedAccessBlobPolicy();
            sasContraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            sasContraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            sasContraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write;

            var sasBlobToken = blob.GetSharedAccessSignature(sasContraints, null, null, null, null);

            return new OkObjectResult(sasBlobToken);
        }
    }
}

