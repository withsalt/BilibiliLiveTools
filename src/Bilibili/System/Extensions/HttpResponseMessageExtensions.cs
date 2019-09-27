using Bilibili.Helper;
using Bilibili.Model;
using Bilibili.Model.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bilibili.System.Extensions
{
    static class HttpResponseMessageExtensions
    {
        public static async Task<ResultModel<T>> ConvertResultModel<T>(this HttpResponseMessage httpResponse) where T: IResultData
        {
            try
            {
                string result = await httpResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("result data is null.");
                }
                JObject jObject = JObject.Parse(result);
                //如果data结点是空的话，返回的是一个空数组
                if(jObject["data"].Type == JTokenType.Array)
                {
                    result =  result.Replace("\"data\":[]", "\"data\":null");
                }
                return JsonHelper.DeserializeJsonToObject<ResultModel<T>>(result);
            }
            catch(Exception ex)
            {
                throw new Exception($"Convert result model failed. {ex.Message}");
            }
        }
    }
}
