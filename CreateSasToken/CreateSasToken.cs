using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CreateSasToken
{
    public static class CreateSasToken
    {
        [FunctionName("CreateSasToken")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string blobName = req.Query["name"];
            var accountKey = Environment.GetEnvironmentVariable("AccountKey");
            var contaunerName = Environment.GetEnvironmentVariable("BlobName");
            try
            {
                var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable(accountKey));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(contaunerName);
                container.CreateIfNotExistsAsync();
                var blob = container.GetBlockBlobReference(blobName);

                var sasContraints = new SharedAccessBlobPolicy
                {
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write
                };

                var ipRange = new IPAddressOrRange("192.168.192.168");

                var sasBlobToken = blob.GetSharedAccessSignature(sasContraints, null, null, null, ipRange);

                return new OkObjectResult(sasBlobToken);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while generating the SAS token.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}


