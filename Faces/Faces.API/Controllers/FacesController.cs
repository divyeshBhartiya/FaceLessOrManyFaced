using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Faces.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacesController : ControllerBase
    {
        /// <summary>
        /// For Orders API project.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>Return Tuple of List of Byte Arrays and Guid Order ID</returns>
        [HttpPost]
        public async Task<Tuple<List<byte[]>, Guid>> ReadFaces(Guid orderId)
        {
            using var ms = new MemoryStream(2048);
            await Request.Body.CopyToAsync(ms);
            var faces = GetFaces(ms.ToArray());
            return new Tuple<List<byte[]>, Guid>(faces, orderId);
        }

        // For Faces.API.Tests project
        //[HttpPost]
        //public async Task<List<byte[]>> ReadFaces()
        //{
        //    using var ms = new MemoryStream(2048);
        //    await Request.Body.CopyToAsync(ms);
        //    var faces = GetFaces(ms.ToArray());
        //    return faces;
        //}

        private List<byte[]> GetFaces(byte[] image)
        {
            Mat src = Cv2.ImDecode(image, ImreadModes.Color);
            // Convert the byte array into jpeg image and Save the image coming from the source
            //in the root directory for testing purposes. 
            src.SaveImage("image.jpg", new ImageEncodingParam(ImwriteFlags.JpegProgressive, 255));
            var file = Path.Combine(Directory.GetCurrentDirectory(), "CascadeFile", "haarcascade_frontalface_default.xml");
            var faceCascade = new CascadeClassifier();
            faceCascade.Load(file);
            var faces = faceCascade.DetectMultiScale(src, 1.1, 6, HaarDetectionTypes.DoRoughSearch, new Size(60, 60));
            var faceList = new List<byte[]>();
            int j = 0;
            foreach (var rect in faces)
            {
                var faceImage = new Mat(src, rect);
                faceList.Add(faceImage.ToBytes(".jpg"));
                faceImage.SaveImage("face" + j + ".jpg", new ImageEncodingParam(ImwriteFlags.JpegProgressive, 255));
                j++;
            }
            return faceList;

        }
    }
}
