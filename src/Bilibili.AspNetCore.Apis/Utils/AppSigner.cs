using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bilibili.AspNetCore.Apis.Utils
{
    public class AppSigner
    {
        public static string Sign(string appKey, string appSec, Dictionary<string, string> paramsDict)
        {
            if (string.IsNullOrWhiteSpace(appKey))
            {
                throw new ArgumentNullException(nameof(appKey), "App key cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(appSec))
            {
                throw new ArgumentNullException(nameof(appSec), "App secret cannot be null or empty.");
            }

            paramsDict["appkey"] = appKey;
            SortedDictionary<string, string> sortedParams = new SortedDictionary<string, string>(paramsDict);
            StringBuilder queryBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in sortedParams)
            {
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Append('&');
                }

                string encodedKey = HttpUtility.UrlEncode(entry.Key, Encoding.UTF8) ?? string.Empty;
                string encodedValue = HttpUtility.UrlEncode(entry.Value, Encoding.UTF8) ?? string.Empty;

                queryBuilder.Append(encodedKey);
                queryBuilder.Append('=');
                queryBuilder.Append(encodedValue);
            }

            queryBuilder.Append(appSec);
            return GenerateMD5(queryBuilder.ToString());
        }

        private static string GenerateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
