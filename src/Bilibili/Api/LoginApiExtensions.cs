using System;
using System.Linq;
using System.Threading.Tasks;
using Bilibili.Helper;
using Bilibili.Model.User;
using Bilibili.Model.User.LoginResult;
using Newtonsoft.Json.Linq;

namespace Bilibili.Api
{
    /// <summary />
    public class LoginDataUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// 登录数据出现更新的用户
        /// </summary>
        public User User { get; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="user"></param>
        public LoginDataUpdatedEventArgs(User user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }
    }

    /// <summary>
    /// 登录API扩展类，提供快速操作
    /// </summary>
    public static class LoginApiExtensions
    {
        /// <summary>
        /// 在用户登录数据更新时发生
        /// </summary>
        public static event EventHandler<LoginDataUpdatedEventArgs> LoginDataUpdated;

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public static async Task<bool> Login(this User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            bool flag;
            int expiresIn;
            string key;
            string json;
            ResultInfo result;

            if (user.HasData)
            {
                // 如果以前登录过，判断一下需不需要重新登录
                // 这个API每次登录有效时间是720小时（expires_in=2592000）
                (flag, expiresIn) = await user.GetExpiresIn();
                if (flag)
                {
                    // Token有效
                    if (expiresIn < 1800)
                    {
                        // Token有效，但是有效时间太短，小于半个小时
                        user.LogWarning("Token有效时间不足");
                        return user.IsLogin = await user.RefreshToken();
                    }
                    else
                    {
                        // Token有效时间足够
                        user.LogInfo("使用缓存Token登录成功");
                        user.LogInfo($"Token有效时间还剩：{Math.Round(expiresIn / 3600d, 1)} 小时");
                        return user.IsLogin = true;
                    }
                }
            }
            // 不存在登录数据，这是第一次登录
            try
            {
                key = await LoginApi.GetKeyAsync(user);
                json = await LoginApi.LoginAsync(user, key, null);
                if (string.IsNullOrEmpty(json))
                {
                    throw new ApiException(new Exception("登录失败，没有返回的数据。"));
                }
                result = JsonHelper.DeserializeJsonToObject<ResultInfo>(json);
            }
            catch (Exception ex)
            {
                user.LogError("登录失败");
                throw new ApiException(ex);
            }
            if (result.Code == 0 && result.Data.Status == 0)
            {
                // 登录成功，保存数据直接返回
                user.LogInfo("登录成功");
                UpdateLoginData(user, result);
                OnLoginDataUpdated(new LoginDataUpdatedEventArgs(user));
                return user.IsLogin = true;
            }
            else if (result.Code == -105)
                // 需要验证码
                return await LoginWithCaptcha(user, key);
            else
            {
                // 其它错误
                user.LogError("登录失败");
                user.LogError($"错误信息：{Utils.FormatJson(json)}");
                return false;
            }
        }

        /// <summary>
        /// 尝试获取Token过期时间
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public static async Task<(bool, int)> GetExpiresIn(this User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            ResultInfo result;

            try
            {
                string json = await LoginApi.GetInfoAsync(user);
                if (string.IsNullOrEmpty(json))
                {
                    throw new ApiException(new Exception("获取Token过期时间失败，没有返回的数据。"));
                }
                result = JsonHelper.DeserializeJsonToObject<ResultInfo>(json);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
            if (result.Code == 0 && !string.IsNullOrEmpty(result.Data.Mid))
            {
                return (true, result.Data.ExpiresIn);
            }
            else
            {
                return (false, 0);
            }
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public static async Task<bool> RefreshToken(this User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            string json;
            ResultInfo result;

            try
            {
                json = await LoginApi.RefreshTokenAsync(user);
                if (string.IsNullOrEmpty(json))
                {
                    throw new ApiException(new Exception("登录失败，没有返回的数据。"));
                }
                result = JsonHelper.DeserializeJsonToObject<ResultInfo>(json);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
            if (result.Code == 0 && !string.IsNullOrEmpty(result.Data.TokenInfo.Mid))
            {
                user.LogInfo("Token刷新成功");
                UpdateLoginData(user, result);
                OnLoginDataUpdated(new LoginDataUpdatedEventArgs(user));
                return true;
            }
            else
            {
                user.LogError("Token刷新失败");
                user.LogError($"错误信息：{Utils.FormatJson(json)}");
                return false;
            }
        }

        private static async Task<bool> LoginWithCaptcha(User user, string key)
        {
            string json;
            ResultInfo result;

            try
            {
                string captcha;

                captcha = await LoginApi.SolveCaptchaAsync(await LoginApi.GetCaptchaAsync(user));
                json = await LoginApi.LoginAsync(user, key, captcha);
                if (string.IsNullOrEmpty(json))
                {
                    throw new ApiException(new Exception("登录失败，没有返回的数据。"));
                }
                result = JsonHelper.DeserializeJsonToObject<ResultInfo>(json);
            }
            catch (Exception ex)
            {
                user.LogError("登录失败");
                throw new ApiException(ex);
            }
            if (result.Code == 0 && result.Data.Status == 0)
            {
                // 登录成功，保存数据直接返回
                user.LogInfo("登录成功");
                UpdateLoginData(user, result);
                OnLoginDataUpdated(new LoginDataUpdatedEventArgs(user));
                return true;
            }
            else
            {
                // 其它错误
                user.LogError("登录失败");
                user.LogError($"错误信息：{Utils.FormatJson(json)}");
                return false;
            }
        }

        private static void UpdateLoginData(User user, ResultInfo result)
        {
            try
            {
                if (user.Data == null)
                {
                    user.Data = new LoginData();
                }

                LoginResultData data = result.Data;

                user.Data.AccessKey = data.TokenInfo.AccessToken;
                user.Data.Cookie = string.Join(";", data.CookieInfo.Cookies.Select(t => t.Name + "=" + t.Value));
                CookiesItem cookie = data.CookieInfo.Cookies.SingleOrDefault(t => t.Name == "bili_jct");
                if (cookie == null)
                {
                    throw new Exception("Can not get csrf by cookie.");
                }
                user.Data.Csrf = cookie.Value;
                user.Data.RefreshToken = data.TokenInfo.RefreshToken;
                cookie = data.CookieInfo.Cookies.SingleOrDefault(t => t.Name == "DedeUserID");
                if (cookie == null)
                {
                    throw new Exception("Can not get uid by cookie.");
                }
                user.Data.Uid = data.CookieInfo.Cookies.SingleOrDefault(t => t.Name == "DedeUserID").Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Update user login data failed. {ex.Message}");
            }
        }

        private static void OnLoginDataUpdated(LoginDataUpdatedEventArgs e)
        {
            LoginDataUpdated?.Invoke(null, e);
        }
    }
}
