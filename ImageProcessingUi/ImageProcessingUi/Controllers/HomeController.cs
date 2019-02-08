using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ImageProcessingUi.Models;
using System.Net.Http;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace ImageProcessingUi.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost("UploadFile")]
        public IActionResult upload(IFormFile formFile, string command)
        {

            var filePath = "C:/Users/Images/Input";



            if (command.Equals("Upload & Convert jpg to png"))
            {
                using (var bits = new FileStream(Path.Combine(filePath, formFile.FileName), FileMode.Create))
                {
                    formFile.CopyToAsync(bits);
                    bits.Flush();
                }
                ImageProcess();
            }
            else
            {
                UploadToBlob();
            }


            return View("ImageProcess");

        }


        public IActionResult ImageProcess()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44369/api/ImageSharp");
                var ResponseTask = client.GetAsync("");
                ResponseTask.Wait();

                var result = ResponseTask.Result;

            }
            return View();
        }

        public IActionResult UploadToBlob()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44369/api/UploadToBlob");
                var ResponseTask = client.GetAsync("");
                ResponseTask.Wait();

                var result = ResponseTask.Result;

            }
            return View();
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
