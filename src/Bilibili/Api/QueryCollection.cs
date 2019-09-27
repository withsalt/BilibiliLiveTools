using System.Collections.Generic;

namespace Bilibili.Api {
	internal sealed class QueryCollection : List<KeyValuePair<string, string>> {
		public void Add(string key, string value) {
			Add(new KeyValuePair<string, string>(key, value));
		}
	}
}
