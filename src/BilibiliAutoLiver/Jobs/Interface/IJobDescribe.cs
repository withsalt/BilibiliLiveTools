using BilibiliAutoLiver.Models;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Interface
{
    public interface IJobDescribe
    {
        IJobDetail JobDetail { get; }

        ITrigger CreateTrigger();

    }
}
