using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace AzFuncServerlessDataBlobIngress
{
    public static class DataBlobingress
    {

        private const string ContainerName = "DataIngressUpload";
        private const string FileName = "FileName";

        [FunctionName("DataBlobingress")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest request,
            // // Dynamic output bindings for Blob folder
            Binder binder,
            CancellationToken token,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Content type validation 

            if (request.ContentType != "application/json")
            {
                return new BadRequestResult();
            }

            if (token.IsCancellationRequested)
            {
                log.LogError("Function was cancelled.");
            }

            // Define Blob Location 

            var BlobPath = GetBlobPath();

            // Upload to blob with stream 

            using (var output = await binder.BindAsync<Stream>(new BlobAttribute(BlobPath, FileAccess.Write), token))
            {
                await request.Body.CopyToAsync(output, token);
            };

            string responseMessage = string.IsNullOrEmpty(BlobPath)
                ? "This HTTP triggered function executed successfully. "
                : $" Location: {BlobPath}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }


        #region helpers

        private static string GetBlobPath()
        {
            // ISO8601 date formatting

            var ingressDate = DateTime.UtcNow;
            var folderPath = $"{ContainerName}/{ingressDate:yyyy-MM-dd}".ToLower();
            var DynamicFileName = $"{FileName}_{ingressDate:yyyy-MM-ddTHH:mm:ss.fff}.json".ToLower();
            var Output = $"{folderPath}/{DynamicFileName}";

            return Output;
        }

        #endregion helpers







    }
}