using BilibiliLiver;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiverTests
{
    public class BilibiliLiverTestsBase
    {
        public IServiceProvider ServiceProvider { get; set; }

        public BilibiliLiverTestsBase()
        {
            this.ServiceProvider = Program.CreateHostBuilder(null).Build().Services;
        }
    }
}
