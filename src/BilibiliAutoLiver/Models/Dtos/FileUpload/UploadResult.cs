using System.Collections.Generic;

namespace PiPlayer.Models.ViewModels.Response.FileUpload
{
    public class UploadResult
    {
        public string error { get; set; }

        public List<string> errorkeys { get; set; } = new List<string>();

        public List<string> initialPreview { get; set; } = new List<string>();

        public List<string> initialPreviewConfig { get; set; } = new List<string>();

        public List<string> initialPreviewThumbTags { get; set; } = new List<string>();

        public bool append { get; set; } = true;
    }
}
