using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Bilibili.AspNetCore.Apis.Exceptions;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;
using Bilibili.AspNetCore.Apis.Utils;
using Microsoft.Extensions.Logging;

namespace Bilibili.AspNetCore.Apis.Services.Cookie
{
    internal class BilibiliCookieBuilder
    {
        private readonly IHttpClientService _httpClient;
        private readonly ILogger<BilibiliCookieService> _logger;

        /// <summary>
        /// 获取设备指纹
        /// </summary>
        private const string _getbuvid = "https://api.bilibili.com/x/frontend/finger/spi";

        /// <summary>
        /// 获取bili_ticket
        /// </summary>
        private const string _getBiliTicket = "https://api.bilibili.com/bapis/bilibili.api.ticket.v1.Ticket/GenWebTicket";

        /// <summary>
        /// 激活buvid3 buvid4
        /// </summary>
        private const string _activeBuvidfp = "https://api.bilibili.com/x/internal/gaia-gateway/ExClimbWuzhi";

        private readonly CookiesJson _cookiesJson;
        private readonly List<CookieHeaderValue> _cookies = new List<CookieHeaderValue>();
        private readonly ulong _timestamp = (ulong)(DateTimeOffset.Now.AddMinutes(-1).ToUnixTimeMilliseconds() / 1000);

        public BilibiliCookieBuilder(ILogger<BilibiliCookieService> logger
            , IHttpClientService httpClient
            , string cookieStr)
        {
            _logger = logger;
            _httpClient = httpClient;

            //转换模型
            _cookiesJson = JsonUtil.DeserializeJsonToObject<CookiesJson>(cookieStr);
            if (_cookiesJson.Cookies?.Any() != true)
            {
                throw new ArgumentNullException("没有Cookie项目");
            }
            foreach (var item in _cookiesJson.Cookies)
            {
                if (CookieHeaderValue.TryParse(item, out var value))
                {
                    _cookies.Add(value);
                }
                else
                {
                    throw new Exception($"Try parse \"{item}\" to cookie header value failed.");
                }
            }
            var dedeUserID = _cookies.Where(p => p.Cookies.Any(t => t.Name.Equals("DedeUserID", StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
            if (dedeUserID != null && dedeUserID.Expires != null)
            {
                if (dedeUserID.Expires <= DateTime.UtcNow)
                {
                    throw new CookieExpiredException($"当前保存的Cookie已过期，过期时间：{dedeUserID.Expires.Value.ToLocalTime()}");
                }
            }
        }

        public BilibiliCookieBuilder SetBnut()
        {
            if (_cookies.Any(p => p.Cookies?.Any(a => a.Name.Equals("b_nut", StringComparison.OrdinalIgnoreCase)) == true))
            {
                return this;
            }
            var b_nut = CopyFromExistCookie("b_nut", _timestamp.ToString());
            _cookies.Add(b_nut);
            return this;
        }

        private string _uuid = null;

        public BilibiliCookieBuilder SetUuid()
        {
            if (_cookies.Any(p => p.Cookies?.Any(a => a.Name.Equals("_uuid", StringComparison.OrdinalIgnoreCase)) == true))
            {
                return this;
            }
            _uuid = GetCookieUuid();
            var buvid3 = CopyFromExistCookie("_uuid", _uuid);
            _cookies.Add(buvid3);
            return this;
        }

        public BilibiliCookieBuilder SetLsid()
        {
            if (_cookies.Any(p => p.Cookies?.Any(a => a.Name.Equals("b_lsid", StringComparison.OrdinalIgnoreCase)) == true))
            {
                return this;
            }
            var blsid = GenBLsid(_timestamp * 1000);
            var b_lsid = CopyFromExistCookie("b_lsid", blsid);
            _cookies.Add(b_lsid);
            return this;
        }

        public BilibiliCookieBuilder SetBuvid3_4()
        {
            var buvidInfo = GetBuvid().ConfigureAwait(false).GetAwaiter().GetResult();
            if (!_cookies.Any(p => p.Cookies?.Any(a => a.Name.Equals("buvid3", StringComparison.OrdinalIgnoreCase)) == true) && !string.IsNullOrWhiteSpace(buvidInfo.b_3))
            {
                var buvid3 = CopyFromExistCookie("buvid3", buvidInfo.b_3);
                _cookies.Add(buvid3);
            }
            if (!_cookies.Any(p => p.Cookies?.Any(a => a.Name.Equals("buvid4", StringComparison.OrdinalIgnoreCase)) == true) && !string.IsNullOrWhiteSpace(buvidInfo.b_4))
            {
                var buvid4 = CopyFromExistCookie("buvid4", buvidInfo.b_3);
                _cookies.Add(buvid4);
            }
            return this;
        }

        public BilibiliCookieBuilder SetBuvidFp()
        {
            if (_cookies.Any(p => p.Cookies?.Any(a => a.Name.Equals("buvid_fp", StringComparison.OrdinalIgnoreCase)) == true))
            {
                return this;
            }
            if (string.IsNullOrEmpty(_uuid))
            {
                throw new ArgumentException("请先调用SetUuid获取UUID");
            }
            if (!_cookies.Any(p => p.Cookies?.Any(a => a.Name.Equals("buvid3", StringComparison.OrdinalIgnoreCase)) == true))
            {
                throw new ArgumentException("请先调用SetBuvid3_4获取buvid3/buvid4");
            }
            Buvid3_4Calculator calculator = new Buvid3_4Calculator(_uuid, _timestamp);
            var payload = calculator.GetPayload();
            var fp = calculator.Generate();
            var buvid_fp_plain = CopyFromExistCookie("buvid_fp_plain", "undefined");
            _cookies.Add(buvid_fp_plain);
            var buvid_fp = CopyFromExistCookie("buvid_fp", fp);
            _cookies.Add(buvid_fp);
            var fingerprint = CopyFromExistCookie("fingerprint", fp);
            _cookies.Add(fingerprint);

            try
            {
                bool rt = ActiveBuvidfp(payload)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
                if (!rt)
                {
                    throw new Exception("Active buvid_fp失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
            }
            return this;
        }

        private async Task<bool> ActiveBuvidfp(string payload)
        {
            CookiesData cookiesData = new CookiesData()
            {
                Cookies = _cookies,
                RefreshToken = _cookiesJson.RefreshToken,
            };
            string cookie = cookiesData.GetCookieString();
            ResultModel<CookieInfo> result = await _httpClient.Execute<CookieInfo>(_activeBuvidfp, HttpMethod.Post, payload, withCookie: true, cookie: cookie);
            if (result == null || result.Data == null)
            {
                throw new Exception("Active buvid_fp失败，返回数据为空");
            }
            if (result.Code != 0)
            {
                throw new Exception($"Active buvid_fp失败，{result.Message}");
            }
            return true;
        }


        public BilibiliCookieBuilder SetTicket()
        {
            if (_cookies.Any(p => p.Cookies?.Any(a => a.Name.Equals("bili_ticket", StringComparison.OrdinalIgnoreCase)) == true))
            {
                return this;
            }
            try
            {
                var ticketInfo = GetTicket().ConfigureAwait(false).GetAwaiter().GetResult();
                var bili_ticket = CopyFromExistCookie("bili_ticket", ticketInfo.ticket);
                _cookies.Add(bili_ticket);
                var bili_ticket_expires = CopyFromExistCookie("bili_ticket_expires", (_timestamp + (ulong)ticketInfo.ttl).ToString());
                _cookies.Add(bili_ticket_expires);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"获取Ticket失败，{ex.Message}");
            }
            return this;
        }

        public CookiesData Build()
        {
            CookiesData cookiesData = new CookiesData()
            {
                Cookies = _cookies,
                RefreshToken = _cookiesJson.RefreshToken,
            };
            if (cookiesData.TryGetValue("bili_ticket_expires", out string biliTicketExpires) && long.TryParse(biliTicketExpires, out long biliTicketExpiresVal))
            {
                cookiesData.HasTicket = true;
                cookiesData.TicketExpireIn = TimeUtil.UnixTimeStampToDateTime(biliTicketExpiresVal);
            }
            return cookiesData;
        }

        #region private

        private string GenBLsid(ulong timestamp)
        {
            var random = new Random();
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                ret.Append(random.Next(0, 16).ToString("X"));
            }
            ret.Append("_");
            ret.Append(timestamp.ToString("X"));
            return ret.ToString();
        }

        private async Task<TicketInfo> GetTicket()
        {
            string key = "XgwSnGZ1p";
            string ts = (DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeMilliseconds() / 1000).ToString();
            string tsStr = $"ts{ts}";
            string hash = HmacSha256(key, tsStr);

            Dictionary<string, string> param = new Dictionary<string, string>()
            {
                { "key_id", "ec02" },
                { "hexsign", hash },
                { "context[ts]", ts },
                { "csrf", "" },
            };

            string urlParams = $"key_id=ec02&hexsign={hash}&{HttpUtility.UrlEncode("context[ts]")}={ts}&csrf=";
            string url = _getBiliTicket.Contains("?") ? (_getBiliTicket + "&" + urlParams) : (_getBiliTicket + "?" + urlParams);
            ResultModel<TicketInfo> result = await _httpClient.Execute<TicketInfo>(url, HttpMethod.Post, param, BodyFormat.Form_UrlEncoded, withCookie: false);
            if (result == null || result.Data == null)
            {
                throw new Exception("获取Ticket结果为空！");
            }
            if (result.Code != 0)
            {
                throw new Exception($"{result.Message}，Code：{result.Code}");
            }
            return result.Data;
        }

        private string HmacSha256(string key, string message)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private async Task<BuvidInfo> GetBuvid()
        {
            ResultModel<BuvidInfo> result = await _httpClient.Execute<BuvidInfo>(_getbuvid, HttpMethod.Get, withCookie: false);
            if (result == null || result.Data == null)
            {
                throw new Exception("获取buvid结果为空！");
            }
            if (result.Code != 0)
            {
                throw new Exception($"获取buvid失败：{result.Message}");
            }
            if (string.IsNullOrWhiteSpace(result.Data.b_3) || string.IsNullOrWhiteSpace(result.Data.b_4))
            {
                throw new Exception($"获取buvid失败：获取到的buvid3或buvid4为空");
            }
            return result.Data;
        }

        private CookieHeaderValue CopyFromExistCookie(string name, string value)
        {
            var result = new CookieHeaderValue(name, value)
            {
                Expires = _cookies[0].Expires,
                MaxAge = _cookies[0].MaxAge,
                Domain = _cookies[0].Domain,
                Path = _cookies[0].Path,
                Secure = _cookies[0].Secure,
                HttpOnly = _cookies[0].HttpOnly,
            };
            return result;
        }
        private string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        private string GetCookieUuid()
        {
            const int LEN = 16;
            string[] DIGHT_MAP = new string[LEN] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "10" };
            long t = DateTimeOffset.Now.ToUnixTimeMilliseconds() % 100_000;
            Random rng = new Random();
            byte[] index = new byte[32];
            rng.NextBytes(index);
            StringBuilder result = new StringBuilder(64);
            for (int ii = 0; ii < index.Length; ii++)
            {
                if (new int[] { 9, 13, 17, 21 }.Contains(ii))
                {
                    result.Append('-');
                }
                result.Append(DIGHT_MAP[index[ii] & 0x0f]);
            }
            return string.Format("{0}{1:D5}infoc", result, t);
        }
        #endregion
    }
}
