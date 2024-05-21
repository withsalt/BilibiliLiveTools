using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BilibiliAutoLiver.Extensions
{
    public static class IdleBusExtesion
    {
        static readonly AsyncLocal<string> asyncLocalTenantId = new AsyncLocal<string>();
        public static IdleBus<IFreeSql> ChangeTenant(this IdleBus<IFreeSql> ib, string tenantId)
        {
            asyncLocalTenantId.Value = tenantId;
            return ib;
        }

        /// <summary>
        /// 获取Name为default，或列表中第一个数据库连接
        /// </summary>
        /// <param name="ib"></param>
        /// <returns></returns>
        public static IFreeSql Get(this IdleBus<IFreeSql> ib)
        {
            if (!string.IsNullOrEmpty(asyncLocalTenantId.Value))
            {
                return ib.Get(asyncLocalTenantId.Value);
            }
            List<string> keys = ib.GetKeys()?.OrderBy(p => p)?.ToList();
            if (keys == null || !keys.Any())
            {
                throw new Exception("Not found db in the IdleBus.");
            }
            int defaultIndex = keys.FindIndex(p => p.Equals("default", StringComparison.OrdinalIgnoreCase));
            if (defaultIndex >= 0)
            {
                return ib.Get(keys[defaultIndex]);
            }
            return ib.Get(keys.First());
        }
    }
}
