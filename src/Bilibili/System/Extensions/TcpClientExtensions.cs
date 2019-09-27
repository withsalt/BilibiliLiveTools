using System.Net.Sockets;
using System.Threading.Tasks;

namespace System.Extensions {
	internal static class TcpClientExtensions {
		public static async Task ReceiveExactlyAsync(this TcpClient client, byte[] buffer) {
			await client.ReceiveExactlyAsync(buffer, 0, buffer.Length);
		}

		public static async Task ReceiveExactlyAsync(this TcpClient client, byte[] buffer, int offset, int count) {
			do {
				int numberOfBytesReceived;

				numberOfBytesReceived = await client.ReceiveAsync(buffer, offset, count);
				offset += numberOfBytesReceived;
				count -= numberOfBytesReceived;
			} while (count != 0);
		}

		public static Task<int> ReceiveAsync(this TcpClient client, byte[] buffer, int offset, int count) {
			if (client == null)
				throw new ArgumentNullException(nameof(client));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			return client.GetStream().ReadAsync(buffer, offset, count);
		}

		public static Task SendAsync(this TcpClient client, byte[] buffer) {
			return client.SendAsync(buffer, 0, buffer.Length);
		}

		public static Task SendAsync(this TcpClient client, byte[] buffer, int offset, int count) {
			if (client == null)
				throw new ArgumentNullException(nameof(client));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			return client.GetStream().WriteAsync(buffer, offset, count);
		}
	}
}
