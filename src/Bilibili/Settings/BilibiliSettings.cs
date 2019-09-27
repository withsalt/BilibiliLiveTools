using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bilibili.Settings {
	/// <summary />
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class BilibiliSettings {
#pragma warning disable CS0649
		[JsonProperty("app_secret")]
		private readonly string _appSecret;
		[JsonProperty("General")]
		private readonly Dictionary<string, string> _general;
		[JsonProperty("PCHeaders")]
		private readonly Dictionary<string, string> _pcHeaders;
		[JsonProperty("AppHeaders")]
		private readonly Dictionary<string, string> _appHeaders;
#pragma warning restore CS0649

		/// <summary />
		public string AppSecret => _appSecret;

		/// <summary />
		public Dictionary<string, string> General => _general;

		/// <summary />
		public Dictionary<string, string> PCHeaders => _pcHeaders;

		/// <summary />
		public Dictionary<string, string> AppHeaders => _appHeaders;

		/// <summary>
		/// 加载配置
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static BilibiliSettings FromJson(string json) {
			BilibiliSettings settings;

			settings = JsonConvert.DeserializeObject<BilibiliSettings>(json);
			return settings;
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
