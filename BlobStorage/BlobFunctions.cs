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
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;

namespace BlobStorage
{
    public static class BlobFunctions
    {
        [FunctionName("UploadFile")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequestMessage req)
        {
            dynamic data = await req.Content.ReadAsAsync<object>();
            string base64String = data.base64;
            string fileName = data.fileName;
            Uri uri = await UploadBlobAsync(base64String,fileName);
            return req.CreateResponse(HttpStatusCode.Accepted, uri);
        }

        [FunctionName("DownloadFile")]
        public static async Task<HttpResponseMessage> Download(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            dynamic data = await req.Content.ReadAsAsync<object>();
            string url = data.url;
            string fileName = data.fileName;
            await DownloadBlobAsync(url, fileName);
            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        private static Match GetMatch(string base64, string type)
        {
            if(type == "image")
            {
                return new Regex(
                              $@"^data\:(?<type>image\/(jpg|gif|png));base64,(?<data>[A-Z0-9\+\/\=]+)$",
                              RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)
                              .Match(base64);
            }

            if (type == "pdf")
            {
                return new Regex(
                              $@"^data\:(?<type>application\/(pdf));base64,(?<data>[A-Z0-9\+\/\=]+)$",
                              RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)
                              .Match(base64);
            }

            if(type == "txt")
            {
                return new Regex(
                              $@"^data\:(?<type>text\/(plain));base64,(?<data>[A-Z0-9\+\/\=]+)$",
                              RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)
                              .Match(base64);
            }

            return new Regex(
              $@"^data\:(?<type>image\/(jpg|gif|png));base64,(?<data>[A-Z0-9\+\/\=]+)$",
              RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)
              .Match(base64);
        }

        public static async Task<Uri> UploadBlobAsync(string base64String,string fileName)
        {

            var match = GetMatch(base64String,"pdf");
            string contentType = match.Groups["type"].Value;
            string extension = contentType.Split('/')[1];
            fileName = $"{fileName}.{extension}";
 
            byte[] photoBytes = Convert.FromBase64String(match.Groups["data"].Value);

            CloudStorageAccount storageAccount = 
              CloudStorageAccount.Parse("XXXX");
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("files");

            await container.CreateIfNotExistsAsync(
              BlobContainerPublicAccessType.Blob,
              new BlobRequestOptions(),
              new OperationContext());
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = contentType;

            using (Stream stream = new MemoryStream(photoBytes, 0, photoBytes.Length))
            {
                await blob.UploadFromStreamAsync(stream).ConfigureAwait(false);
            }

            return blob.Uri;
        }

        public static async Task DownloadBlobAsync(string url, string fileName)
        {
            
            CloudStorageAccount storageAccount =
              CloudStorageAccount.Parse("XXXXX");
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("files");

            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

            await blob.DownloadToFileAsync(@"C:\temp\"+fileName, FileMode.OpenOrCreate);

        }
    }
}
