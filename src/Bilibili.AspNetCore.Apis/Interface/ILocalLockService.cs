using System;

namespace Bilibili.AspNetCore.Apis.Interface
{
    public interface ILocalLockService
    {
        /// <summary>
        /// 尝试加锁，并不进行加锁
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsLocked(string key);

        /// <summary>
        /// 加锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expired"></param>
        /// <returns>成功返回true，失败返回false</returns>
        bool Lock(string key, TimeSpan expired);

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool UnLock(string key);

        /// <summary>
        /// 自旋锁解锁
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool SpinUnLock(string key);

        /// <summary>
        /// 自旋锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        bool SpinLock(string key, int timeoutSeconds);
    }
}
