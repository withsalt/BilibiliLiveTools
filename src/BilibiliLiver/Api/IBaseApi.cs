using BiliAccount;
using BiliAccount.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Api
{
    public class IBaseApi
    {
        public IEnumerable<KeyValuePair<string, string>> PCHeaders { get; set; } = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Accept","application/json, text/plain, */*"),
            new KeyValuePair<string, string>("Accept-Encoding","deflate"),
            new KeyValuePair<string, string>("Accept-Language","zh-CN,zh;q:0.9"),
            new KeyValuePair<string, string>("User-Agent","Mozilla/5.0 BiliDroid/5.57.2 (ccc@gmail.com) os/android model/MI 9 mobi_app/android build/5572000 channel/master innerVer/5572000 osVer/10 network/2"),
        };

        protected string GetCookie(Account account)
        {
            if (account.Expires_Cookies != DateTime.MinValue
                && account.Expires_Cookies.AddMinutes(-10) > DateTime.Now
                && account.Cookies != null
                && account.Cookies.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Cookie item in account.Cookies)
                {
                    sb.Append($"{item.Name}={item.Value}; ");
                }
                return sb.ToString();
            }
            //get sso data
            object[] sso = ByPassword.SSO(account.AccessToken);
            if (sso == null || sso.Length == 0)
            {
                throw new Exception("Get cookie by sso failed.");
            }
            CookieCollection cookies = null;
            DateTime? expiresDt = null;
            foreach (var item in sso)
            {
                Type type = item.GetType();
                if (type.Name == "CookieCollection")
                {
                    cookies = (CookieCollection)item;
                }
                if (type.Name == "DateTime")
                {
                    expiresDt = (DateTime)item;
                }
            }
            if (cookies == null || cookies.Count == 0 || expiresDt == null)
            {
                throw new Exception("Get cookie by sso failed.");
            }
            //update account
            account.Cookies = cookies;
            account.Expires_Cookies = expiresDt.Value;

            //toString
            StringBuilder sbSSO = new StringBuilder();
            foreach (Cookie item in cookies)
            {
                sbSSO.Append($"{item.Name}={item.Value}; ");
            }
            return sbSSO.ToString();
        }
    }
}
