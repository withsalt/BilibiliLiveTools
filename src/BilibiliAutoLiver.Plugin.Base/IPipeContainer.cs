using System;
using System.Collections.Generic;

namespace BilibiliAutoLiver.Plugin.Base
{
    public interface IPipeContainer : IDisposable
    {
        IEnumerable<IPipeProcess> Get();
    }
}
