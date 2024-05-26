using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace PiPlayer.Models.ViewModels.Response.FileUpload
{
    public class UploadParams
    {
        public string fileId { get; set; }

        public string fileName { get; set; }

        public double fileSize { get; set; }

        public List<IFormFile> uploadFiles { get; set; } = new List<IFormFile>();
    }
}
