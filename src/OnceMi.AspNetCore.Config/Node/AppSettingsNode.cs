using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.Config
{
    public class AppSettingsNode
    {
        public bool IsDebug { get; set; }

        public bool IsSeedDatabase { get; set; }

        public int AppId { get; set; }
    }
}
