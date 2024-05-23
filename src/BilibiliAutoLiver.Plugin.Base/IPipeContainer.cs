using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Plugin.Base
{
    public interface IPipeContainer : IDisposable
    {
        IEnumerable<IPipeProcess> Get();
    }
}
