using BilibiliAutoLiver.Extensions;
using FreeSql;

namespace BilibiliAutoLiver.Repository.Base
{
    public class BaseUnitOfWorkManager : UnitOfWorkManager
    {
        public IdleBus<IFreeSql> DbContainer { get; set; }

        public BaseUnitOfWorkManager(IdleBus<IFreeSql> ib) : base(ib.Get())
        {
            DbContainer = ib;
        }
    }
}
