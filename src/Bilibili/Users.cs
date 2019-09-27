using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bilibili {
	/// <summary>
	/// 用户列表
	/// </summary>
	public sealed class Users : List<User> {
		/// <summary>
		/// 加载配置
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static Users FromJson(string json) {
			Users users;

			users = JsonConvert.DeserializeObject<Users>(json);
			foreach (User user in users)
				user.ImportLoginData();
			return users;
		}

		/// <summary>
		/// 转换为JSON
		/// </summary>
		/// <returns></returns>
		public string ToJson() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}
	}
}
