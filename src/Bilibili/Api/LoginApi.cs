using System;
using System.Collections.Generic;
using System.Extensions;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Bilibili.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bilibili.Api
{
    /// <summary>
    /// 登录API
    /// </summary>
    public static class LoginApi
    {
        private const string CAPTCHA_URL = "https://passport.bilibili.com/captcha";
        private const string LOGIN_EXIT_URL = "https://passport.bilibili.com/login?act=exit";
        private const string OAUTH2_GETKEY_URL = "https://passport.bilibili.com/api/oauth2/getKey";
        private const string OAUTH2_INFO_URL = "https://passport.bilibili.com/api/v2/oauth2/info";
        private const string OAUTH2_LOGIN_URL = "https://passport.bilibili.com/api/v2/oauth2/login";
        private const string OAUTH2_REFRESH_TOKEN_URL = "https://passport.bilibili.com/api/v2/oauth2/refresh_token";
        private const string SOLVE_CAPTCHA_URL = "http://115.159.205.242:19951/captcha/v1";

        private static Dictionary<string, string> General => GlobalSettings.Bilibili.General;

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public static async Task<byte[]> GetCaptchaAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Get, CAPTCHA_URL))
                return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public static async Task<bool> LogoutAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Get, LOGIN_EXIT_URL, null, user.PCHeaders))
                return true;
        }

        /// <summary>
        /// 获取Key
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public static async Task<string> GetKeyAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            QueryCollection queries;

            queries = new QueryCollection {
                { "appkey", General["appkey"] }
            };
            queries.SortAndSign();
            using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Post, OAUTH2_GETKEY_URL, queries, null))
                return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public static async Task<string> GetInfoAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            QueryCollection queries;

            queries = new QueryCollection {
                { "access_key", user.Data.AccessKey },
                { "ts", ApiUtils.GetTimeStamp().ToString() }
            };
            queries.AddRange(user.Data.Cookie.Split(';').Select(item =>
            {
                string[] pair;

                pair = item.Split('=');
                return new KeyValuePair<string, string>(pair[0], pair[1]);
            }));
            queries.AddRange(General);
            queries.SortAndSign();
            using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Get, OAUTH2_INFO_URL, queries, user.AppHeaders))
                return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="jsonKey">Key</param>
        /// <param name="captcha">验证码</param>
        /// <returns></returns>
        public static async Task<string> LoginAsync(User user, string jsonKey, string captcha)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(jsonKey))
                throw new ArgumentNullException(nameof(jsonKey));

            JToken loginKey;
            string rsaKey;
            RSAParameters rsaParameters;
            QueryCollection queries;

            loginKey = JObject.Parse(jsonKey)["data"];
            rsaKey = (string)loginKey["key"];
            rsaParameters = ApiUtils.ParsePublicKey(rsaKey);
            queries = new QueryCollection {
                { "username", user.Account },
                { "password", ApiUtils.RsaEncrypt(loginKey["hash"] + user.Password, rsaParameters) },
                { "captcha", captcha ?? string.Empty }
            };
            queries.AddRange(General);
            queries.SortAndSign();
            using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Post, OAUTH2_LOGIN_URL, queries, null))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public static async Task<string> RefreshTokenAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            QueryCollection queries;

            queries = new QueryCollection {
                { "access_key", user.Data.AccessKey },
                { "refresh_token", user.Data.RefreshToken },
                { "ts", ApiUtils.GetTimeStamp().ToString() }
            };
            queries.AddRange(user.Data.Cookie.Split(';').Select(item =>
            {
                string[] pair;

                pair = item.Split('=');
                return new KeyValuePair<string, string>(pair[0], pair[1]);
            }));
            queries.AddRange(General);
            queries.SortAndSign();
            using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Post, OAUTH2_REFRESH_TOKEN_URL, queries, user.AppHeaders))
                return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 识别验证码
        /// </summary>
        /// <param name="captcha"></param>
        /// <returns></returns>
        public static async Task<string> SolveCaptchaAsync(byte[] captcha)
        {
            string json;

            json = JsonConvert.SerializeObject(new
            {
                image = Convert.ToBase64String(captcha)
            });
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Post, SOLVE_CAPTCHA_URL, null, null, null, json, "application/json"))
                return JObject.Parse(await response.Content.ReadAsStringAsync())["message"].ToString();
        }
    }
}
