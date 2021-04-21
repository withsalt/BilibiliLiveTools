using BiliAccount;
using BiliAccount.Linq;
using BilibiliLiver.Config;
using BilibiliLiver.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver
{
    class App
    {
        private readonly ILogger<App> _logger;
        private readonly ConfigManager _config;
        private readonly string _dataFilePath = Path.Combine(AppContext.BaseDirectory, "bilibili.dat");
        private Account _account = null;

        public App(ILogger<App> logger
            , ConfigManager config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task Run(params string[] args)
        {
            //while (true)
            //{
            //    await Task.Delay(1500);

            //    _logger.LogInformation($"Account: {_config.UserSetting.Account}");
            //}
            await LoginToBilibili();


            _logger.LogInformation($"Account: {_account.AccessToken}");
        }


        private async Task LoginToBilibili()
        {
            async Task WriteLoginDataToFile(Account account)
            {
                if (account == null) throw new Exception("Write to file data can not null.");
                string dataBeforeEncrypt = JsonUtil.SerializeObject(account);
                string dataAfterEncrypt = AES128.AESEncrypt(dataBeforeEncrypt, _config.AppSetting.Key, "40863a4f-7cbe-4be2-bb54-765233c83d25");
                await File.WriteAllBytesAsync(_dataFilePath, Encoding.UTF8.GetBytes(dataAfterEncrypt));
            }

            async Task LoginByPassword()
            {
                Account account = ByPassword.LoginByPassword(_config.UserSetting.Account, _config.UserSetting.Password);
                if (account == null)
                {
                    throw new Exception("Get account info failed.");
                }
                if (account.LoginStatus != Account.LoginStatusEnum.ByPassword)
                {
                    throw new Exception($"登录失败，登录失败原因：{account.LoginStatus}");
                }
                await WriteLoginDataToFile(account);
                _account = account;
            }

            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    await LoginByPassword();
                    return;
                }
                //获取历史登录信息
                string fileStr = await File.ReadAllTextAsync(_dataFilePath);
                if (string.IsNullOrEmpty(fileStr))
                {
                    await LoginByPassword();
                    return;
                }
                try
                {
                    string decodeStr = AES128.AESDecrypt(fileStr, _config.AppSetting.Key, "40863a4f-7cbe-4be2-bb54-765233c83d25");
                    if (string.IsNullOrEmpty(decodeStr))
                    {
                        await LoginByPassword();
                        return;
                    }
                    Account account = JsonUtil.DeserializeJsonToObject<Account>(decodeStr);
                    //判断AccessToken是否有效
                    if (!ByPassword.IsTokenAvailable(account.AccessToken))
                    {
                        await LoginByPassword();
                        return;
                    }
                    //判断AccessToken是否需要续期
                    if (account.Expires_AccessToken != DateTime.MinValue
                        && account.Expires_AccessToken.AddDays(-7) < DateTime.Now)
                    {
                        DateTime? dt = ByPassword.RefreshToken(account.AccessToken, account.RefreshToken);
                        if (dt == null)
                        {
                            await LoginByPassword();
                            return;
                        }
                        account.Expires_AccessToken = dt.Value;
                        //更新
                        await WriteLoginDataToFile(account);
                        _account = account;
                    }
                    else
                    {
                        _account = account;
                        return;
                    }
                }
                catch
                {
                    await LoginByPassword();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Login to bilibili failed, login account is {_config.UserSetting.Account}. {ex.Message}", ex);
            }
        }
    }
}
