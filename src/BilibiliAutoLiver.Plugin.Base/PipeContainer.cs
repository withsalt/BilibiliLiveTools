using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Plugin.Base
{
    public class PipeContainer : IPipeContainer
    {
        private static readonly object _locker = new object();

        private SortedList<int, IPipeProcess> Processes = new SortedList<int, IPipeProcess>();

        public void Add(IPipeProcess process)
        {
            lock (_locker)
            {
                if (Processes.ContainsValue(process))
                {
                    return;
                }
                Processes.Add(process.Index, process);
            }
        }

        public IEnumerable<IPipeProcess> Get()
        {
            return this.Processes.Values;
        }
    }
}
