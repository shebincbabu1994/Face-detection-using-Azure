using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Face_Detection.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace Face_Detection.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Detection(IFormFile uploadImage)
        {
            var imageResult = "";
            IFaceClient faceClient = Authenticate("https://centralindia.api.cognitive.microsoft.com/", "YOUR_KEY");
            using (var ms = new MemoryStream())
            {
                uploadImage.CopyTo(ms);
                var fileBytes = ms.ToArray();
                MemoryStream stream = new MemoryStream(fileBytes);
                IList<DetectedFace> faces =  faceClient.Face.DetectWithStreamAsync(stream, true, true, null).GetAwaiter().GetResult();
                using (var drawStream = new MemoryStream())
                {
                    uploadImage.CopyTo(drawStream);
                    using (var img = new Bitmap(drawStream))
                    using (var nonIndexedImg = new Bitmap(img.Width, img.Height))
                    using (var g = Graphics.FromImage(nonIndexedImg))
                    using (var mem = new MemoryStream())
                    {
                        g.DrawImage(img, 0, 0, img.Width, img.Height);

                        var pen = new Pen(Color.Red, 5);

                        foreach (var face in faces)
                        {
                            var rectangle = face.FaceRectangle;

                            g.DrawRectangle(pen, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
                        }

                        nonIndexedImg.Save(mem, ImageFormat.Jpeg);

                        var base64 = Convert.ToBase64String(mem.ToArray());
                        imageResult = String.Format("data:image/png;base64,{0}", base64);
                    }

                }

            }


            return View((object)imageResult);
        }


        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };

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
