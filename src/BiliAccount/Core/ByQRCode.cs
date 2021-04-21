using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

using QRCoder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


#pragma warning disable CS0649

namespace BiliAccount.Core
{
    /// <summary>
    /// 通过二维码登录
    /// </summary>
    internal class ByQRCode
    {
        #region Private Fields

        /// <summary>
        /// 状态监视器
        /// </summary>
        private static Timer Monitor;

        /// <summary>
        /// 刷新监视器
        /// </summary>
        private static Timer Refresher;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// 取消登录
        /// </summary>
        public static void CancelLogin()
        {
            Monitor.Dispose();
            Refresher.Dispose();
        }

        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="Foreground">前景颜色</param>
        /// <param name="Background">背景颜色</param>
        /// <param name="IsBorderVisable">是否使用边框</param>
        /// <returns>二维码位图</returns>
        public static Bitmap GetQrcode(Color Foreground,Color Background,bool IsBorderVisable)
        {
            Bitmap qrCodeImage = null;
        re:
            //获取二维码要包含的url
            string str = Http.GetBody("https://passport.bilibili.com/qrcode/getLoginUrl", null, "https://passport.bilibili.com/login",$"BiliAccount/{Config.Dll_Version}");
            if (!string.IsNullOrEmpty(str))
            {
                GetQrcode_DataTemplete obj = JsonConvert.DeserializeObject<GetQrcode_DataTemplete>(str);
                if (obj.code == 0)
                {
                    // 生成二维码的内容
                    string strCode = obj.data.url;
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(strCode, QRCodeGenerator.ECCLevel.Q);
                    QRCode qrcode = new QRCode(qrCodeData);
                    //生成二维码位图
                    qrCodeImage = qrcode.GetGraphic(5, Foreground, Background, null, 0, 6, IsBorderVisable);

                    //qrCodeImage.MakeTransparent(Background);

                    //if (Background.A != 0)
                    //{
                    //    for (int x = 0; x < qrCodeImage.Width; x++)
                    //    {
                    //        for (int y = 0; y < qrCodeImage.Height; y++)
                    //        {
                    //            if (qrCodeImage.GetPixel(x, y).ToArgb() == 0)
                    //            {
                    //                qrCodeImage.SetPixel(x, y, Background);
                    //            }
                    //        }
                    //    }
                    //}

                    Monitor = new Timer(MonitorCallback, obj.data.oauthKey, 1000, 1000);
                    Refresher = new Timer(RefresherCallback, new List<object>{ Foreground, Background, IsBorderVisable }, 180000, Timeout.Infinite);
                }
            }
            else goto re;

            return qrCodeImage;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// 状态监视器回调
        /// </summary>
        /// <param name="o">oauthKey</param>
        private static void MonitorCallback(object o)
        {
            string oauthKey = o.ToString();

            string str = Http.PostBody("https://passport.bilibili.com/qrcode/getLoginInfo", "oauthKey=" + oauthKey + "&gourl=https%3A%2F%2Fwww.bilibili.com%2F", null, "application/x-www-form-urlencoded; charset=UTF-8", "https://passport.bilibili.com/login",$"BiliAccount/{Config.Dll_Version}");
            if (!string.IsNullOrEmpty(str))
            {
                MonitorCallBack_Templete obj = JsonConvert.DeserializeObject<MonitorCallBack_Templete>(str);
                if (obj.status)
                {
                    //关闭监视器
                    Monitor.Dispose();
                    Refresher.Dispose();

                    Account account = new Account();
                    string Querystring = Regex.Split((obj.data as JObject)["url"].ToString(), "\\?")[1];
                    string[] KeyValuePair = Regex.Split(Querystring, "&");
                    account.Cookies = new CookieCollection();
                    for (int i = 0; i < KeyValuePair.Length - 1; i++)
                    {
                        string[] tmp = Regex.Split(KeyValuePair[i], "=");
                        switch (tmp[0])
                        {
                            case "bili_jct":
                                account.CsrfToken = tmp[1];
                                account.strCookies += KeyValuePair[i] + "; ";
                                account.Cookies.Add(new Cookie(tmp[0], tmp[1]) { Domain = ".bilibili.com" });
                                break;

                            case "DedeUserID":
                                account.Uid = tmp[1];
                                account.strCookies += KeyValuePair[i] + "; ";
                                account.Cookies.Add(new Cookie(tmp[0], tmp[1]) { Domain = ".bilibili.com" });
                                break;

                            case "Expires":
                                account.Expires_Cookies = DateTime.Now.AddSeconds(double.Parse(tmp[1]));
                                break;

                            case "gourl":

                                break;

                            default:
                                account.strCookies += KeyValuePair[i] + "; ";
                                account.Cookies.Add(new Cookie(tmp[0], tmp[1]) { Domain = ".bilibili.com" });
                                break;
                        }
                    }
                    account.strCookies = account.strCookies.Substring(0, account.strCookies.Length - 2);
                    account.LoginStatus = Account.LoginStatusEnum.ByQrCode;
                    Linq.ByQRCode.RaiseQrCodeStatus_Changed(Linq.ByQRCode.QrCodeStatus.Success, account);
                }
                else
                {
                    switch (obj.data)
                    {
                        case -4://未扫描
                            Linq.ByQRCode.RaiseQrCodeStatus_Changed(Linq.ByQRCode.QrCodeStatus.Wating);
                            break;

                        case -5://已扫描
                            Linq.ByQRCode.RaiseQrCodeStatus_Changed(Linq.ByQRCode.QrCodeStatus.Scaned);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 刷新监视器回调
        /// </summary>
        /// <param name="state"></param>
        private static void RefresherCallback(object state)
        {
            Linq.ByQRCode.RaiseQrCodeRefresh(GetQrcode((Color)((List<object>)state)[0], (Color)((List<object>)state)[1], (bool)((List<object>)state)[2]));
        }

        #endregion Private Methods

        #region Private Classes

        /// <summary>
        /// 获取二维码的数据模板
        /// </summary>
        private class GetQrcode_DataTemplete
        {
            #region Public Fields

            public int code;
            public Data_Templete data;

            #endregion Public Fields

            #region Public Classes

            public class Data_Templete
            {
                #region Public Fields

                public string oauthKey;
                public string url;

                #endregion Public Fields
            }

            #endregion Public Classes
        }

        /// <summary>
        /// 状态监视器回调数据模板
        /// </summary>
        private class MonitorCallBack_Templete
        {
            #region Public Fields

            public object data;
            public bool status;

            #endregion Public Fields
        }

        #endregion Private Classes
    }
}