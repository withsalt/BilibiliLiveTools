namespace BilibiliAutoLiver.Config
{
    public class CacheKeyConstant
    {
        /// <summary>
        /// 用于打开页面时，此时正在通过Cookie登录，显示加载中
        /// </summary>
        public const string LOGING_STATUS_CACHE_KEY = "LOGIN_STATUS_CACHE_KEY";

        public const string QRCODE_LOGIN_STATUS_CACHE_KEY = "QRCODE_LOGIN_STATUS_CACHE_KEY";

        public const string LAST_REFRESH_COOKIE_TIME_CACHE_KEY = "LAST_REFRESH_COOKIE_TIME_CACHE_KEY";

        public const string IS_ENABLED_MONITOR_CACHE_KEY = "IS_ENABLED_MONITOR_CACHE_KEY";

        public const string MAIL_SEND_CACHE_KEY = "MAIL_{0}_SEND_STATUS_KEY";

        public const string LIVE_STATUS_CACHE_KEY = "LIVE_STATUS_CACHE_KEY";

        public const string LIVE_LOGS_CACHE_KEY = "LIVE_LOGS_{0}_CACHE_KEY";
    }
}
