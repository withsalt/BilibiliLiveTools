using System.Collections.Generic;
using System.Linq;

namespace System.Extensions {
	internal static class HttpExtensions {
		public static string ToQueryString(this IEnumerable<KeyValuePair<string, string>> queries) {
			if (queries is null)
				throw new ArgumentNullException(nameof(queries));

			return string.Join("&", queries.Select(t => Uri.EscapeDataString(t.Key) + "=" + Uri.EscapeDataString(t.Value)));
		}
	}
}
