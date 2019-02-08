using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessing.Controllers
{
    //https://localhost:44369/api/ImageSharp
    [Route("api/[controller]")]
    [ApiController]
    public class ImageSharpController : ControllerBase
    {      
        [HttpGet]
        public void Get()
        {
            var folderPath = "C:/Users/Images/Input/";
            var files = new DirectoryInfo(folderPath).GetFiles("*.*");
            string latestfile = "";

            DateTime lastupdated = DateTime.MinValue;

            foreach(FileInfo file in files)
            {
                if(file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    latestfile = file.Name;
                }
            }

            latestfile = folderPath + latestfile;

            using (Image<Rgba32> image = Image.Load(latestfile))
                {                                  
                    Stream outputStream = new MemoryStream();
                    image.SaveAsPng(outputStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                    outputStream.Seek(0, SeekOrigin.Begin);

                    String filepath = latestfile.Substring(0, latestfile.IndexOf("Input"));
                    String outpath = filepath + "Output/";
                    String filenamewithoutExt = Path.GetFileNameWithoutExtension(latestfile);

                    using (FileStream fs = new FileStream(outpath + filenamewithoutExt + ".png", FileMode.OpenOrCreate))   
                    {
                        outputStream.CopyTo(fs);
                        fs.Flush();
                    }

                    //var folderPath = "C:/Users/Images/Input/";
                    //System.IO.DirectoryInfo folderInfo = new DirectoryInfo(folderPath);

                    //foreach (FileInfo files in folderInfo.GetFiles())
                    //{
                    //    files.Delete();
                    //}
                }            
            
        }
        
    }
}