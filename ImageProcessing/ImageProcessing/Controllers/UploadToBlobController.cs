using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageProcessing.Controllers
{
    //https://localhost:44369/api/UploadToBlob
    
    [Route("api/[controller]")]
    [ApiController]
    public class UploadToBlobController : ControllerBase
    {
        private IConfiguration _configuration;

        public UploadToBlobController(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }

        // Upload JPG image into BLOB storage
        [HttpGet]
        public async void Get()
        {
            var folderPath = "C:/Users/Images/Input/";
            var files = new DirectoryInfo(folderPath).GetFiles("*.*");
            string latestfile = "";
            string filename = "";

            DateTime lastupdated = DateTime.MinValue;

            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    filename = file.Name;
                }
            }

            latestfile = folderPath + filename;
            bool status;
            using (Image<Rgba32> image = Image.Load(latestfile))
            {
                Stream outputStream = new MemoryStream();
                image.Save(outputStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
                outputStream.Seek(0, SeekOrigin.Begin);
                status = await UploadToBlob(filename, null, outputStream);
            }
               
            //uploadSuccess = await UploadToBlob(filename, null, stream);
        }
        public async Task<bool> UploadToBlob(string filename, byte[] imageBuffer = null, Stream stream = null)
        {
            CloudStorageAccount storageAccount = null;
            CloudBlobContainer cloudBlobContainer = null;
            string storageConnectionString = _configuration["storageconnectionstring"];
            string containerString = _configuration["containerstring"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a container called 'uploadblob' and append a GUID value to it to make the name unique. 
                    cloudBlobContainer = cloudBlobClient.GetContainerReference(containerString.ToString());

                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(filename);

                    if (imageBuffer != null)
                    {
                        // OPTION A: use imageBuffer (converted from memory stream)
                        await cloudBlockBlob.UploadFromByteArrayAsync(imageBuffer, 0, imageBuffer.Length);
                    }
                    else if (stream != null)
                    {
                        // OPTION B: pass in memory stream directly
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                        string downloadPath = await DownloadFromBlob(filename);
                        if (!string.IsNullOrEmpty(downloadPath))
                        {
                            ConvertJpgToPng(downloadPath,filename);
                        }
                        //var newBlob = cloudBlobContainer.GetBlockBlobReference("foo.jpg");
                        //await newBlob.DownloadToFileAsync("fooblob.jpg", FileMode.Create);
                    }
                    else
                    {
                        return false;
                    }

                    return true;


                }
                catch (StorageException ex)
                {
                    return false;
                }
                finally
                {

                }
            }
            else
            {
                return false;
            }

        }

        // Download JPG blob from Azure blob storage and convert into PNG format
        [HttpGet("DownloadFromBlob")]
        public async Task<string> DownloadFromBlob(string filename)
        {
            CloudStorageAccount storageAccount = null;
            CloudBlobContainer cloudBlobContainer = null;
            string storageConnectionString = _configuration["storageconnectionstring"];
            string containerString = _configuration["containerstring"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a container called 'uploadblob' and append a GUID value to it to make the name unique. 
                    cloudBlobContainer = cloudBlobClient.GetContainerReference(containerString.ToString());

                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(filename);


                    //var newBlob = cloudBlobContainer.GetBlockBlobReference("foo.jpg");
                    //await newBlob.DownloadToFileAsync("fooblob.jpg", FileMode.Create);
                    var downloadPath = "C:/Images/Blob/"+filename;
                    var blockBlob = cloudBlobContainer.GetBlockBlobReference(filename);
                    using (var fileStream = System.IO.File.OpenWrite(downloadPath))
                    {
                        await blockBlob.DownloadToStreamAsync(fileStream);
                    }


                    return downloadPath;


                }
                catch (StorageException ex)
                {
                    return "";
                }
                finally
                {
                    // OPTIONAL: Clean up resources, e.g. blob container
                    //if (cloudBlobContainer != null)
                    //{
                    //    await cloudBlobContainer.DeleteIfExistsAsync();
                    //}
                }
            }
            else
            {
                return "";
            }


        }

        public IActionResult ConvertJpgToPng(string url, string filename)
        {
            //Image<Rgba32> sourceImage = await this.LoadImageFromUrl(url);

            Image<Rgba32> sourceImage = Image.Load(url);
            if (sourceImage != null)
            {
                try
                {
                    Stream outputStream = new MemoryStream();
                    sourceImage.SaveAsPng(outputStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                    outputStream.Seek(0, SeekOrigin.Begin);
                    using (FileStream fs = new FileStream(@"C:\\Images\\output\\" + filename+  DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png", FileMode.OpenOrCreate))
                    {
                        outputStream.CopyTo(fs);
                        fs.Flush();
                    }

                    return this.File(outputStream, "image/png");
                }
                catch
                {
                    // Add error logging here
                }
            }
            return this.NotFound();

        }
    }
}