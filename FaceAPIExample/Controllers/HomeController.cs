using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FaceAPIExample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.Extensions.Options;

namespace FaceAPIExample.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private IFaceServiceClient faceServiceClient;
        private readonly AppSettings _appSettings;
        public HomeController(IHostingEnvironment hostingEnvironment,IOptions<AppSettings> appSettings)
        {
            _hostingEnvironment = hostingEnvironment;
            _appSettings = appSettings.Value;
            faceServiceClient = new FaceServiceClient(_appSettings.FaceAPIKey, _appSettings.FaceAPIEndpoint);
        }

        public async Task<Face[]> GetPicFaces(string imageSrc){
            string webRootPath = _hostingEnvironment.WebRootPath;
            return await UploadAndDetectFaces(webRootPath+"/"+imageSrc);
        }
       
        private async Task<Face[]> UploadAndDetectFaces(string imageFilePath)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { 
                FaceAttributeType.Gender, 
                FaceAttributeType.Age, 
                FaceAttributeType.Smile,  
                FaceAttributeType.Glasses, 
                FaceAttributeType.FacialHair
            };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = System.IO.File.OpenRead(imageFilePath))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, 
                                                                       returnFaceId: true, 
                                                                       returnFaceLandmarks: false, 
                                                                       returnFaceAttributes: faceAttributes);
                    return faces;
                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                Trace.WriteLine(f.ToString());
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                return new Face[0];
            }
        }

        
        public IActionResult Index()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;

            if (!Directory.Exists(webRootPath + "/images")){
                throw new DirectoryNotFoundException(webRootPath + "/images");
            }
            var imgDir = new DirectoryInfo(webRootPath+"/images");
            var files = imgDir.EnumerateFiles("*.jpg");

            ViewData["Images"] = files.Select(f => f.Name).OrderBy(fn=>fn).ToList();
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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



    }
}
