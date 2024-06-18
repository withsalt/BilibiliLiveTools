using System.Collections.Generic;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Settings
{
    public class AllowExtension
    {
        public FileType Type { get; set; }

        public List<string> Values { get; set; }
    }
}
