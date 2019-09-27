using System.IO;
using System.Threading.Tasks;

namespace System.Extensions {
	internal static class StreamExtensions {
		public static Task<int> ReadAsync(this Stream stream, byte[] buffer) {
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			return stream.ReadAsync(buffer, 0, buffer.Length);
		}

		public static Task WriteAsync(this Stream stream, byte[] buffer) {
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			return stream.WriteAsync(buffer, 0, buffer.Length);
		}
	}
}
