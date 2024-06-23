namespace BilibiliAutoLiver.Services.Interface
{
    public interface ILocalLockService
    {
        /// <summary>
        /// 尝试加锁，并不进行加锁
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool TryLock(string key);

        bool Lock(string key, int expired);

        bool UnLock(string key);
    }
}
