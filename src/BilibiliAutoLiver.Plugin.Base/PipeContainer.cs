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

        private List<IPipeProcess> Processes = new List<IPipeProcess>();

        public void Add(IPipeProcess process)
        {
            lock (_locker)
            {
                if (Processes.Contains(process))
                {
                    return;
                }
                Processes.Add(process);
                if (Processes.Count > 1)
                {
                    Processes = Processes.OrderBy(p => p.Index).ToList();
                }
            }
        }

        public void Dispose()
        {
            if (this.Processes == null || this.Processes.Count == 0)
            {
                return;
            }
            foreach (IPipeProcess process in Processes)
            {
                process.Dispose();
            }
        }

        public IEnumerable<IPipeProcess> Get()
        {
            return this.Processes;
        }
    }
}
