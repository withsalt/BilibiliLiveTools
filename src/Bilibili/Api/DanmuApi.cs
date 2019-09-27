using System;
using System.Extensions;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bilibili.Api {
	/// <summary>
	/// 弹幕API
	/// </summary>
	public static class DanmuApi {
		private const int HEART_BEAT_INTERVAL_SECONDS = 30;
		private const string DANMU_HOST_NAME = "broadcastlv.chat.bilibili.com";
		private const int DANMU_HOST_PORT = 2243;

		private static readonly Random _random = new Random();
		private static readonly byte[] _heartBeatPacket = Pack(DanmuType.HeartBeat, Array.Empty<byte>());

		/// <summary>
		/// 心跳间隔
		/// </summary>
		public static TimeSpan HeartBeatInterval => new TimeSpan(0, 0, HEART_BEAT_INTERVAL_SECONDS);

		/// <summary>
		/// 连接弹幕服务器
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task ConnectAsync(TcpClient client) {
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			try {
				await client.ConnectAsync(DANMU_HOST_NAME, DANMU_HOST_PORT);
			}
			catch (Exception ex) {
				throw new ApiException(ex);
			}
		}

		/// <summary>
		/// 进入指定房间
		/// </summary>
		/// <param name="client"></param>
		/// <param name="roomId">房间ID</param>
		/// <returns></returns>
		public static async Task EnterRoomAsync(TcpClient client, uint roomId) {
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			try {
				await client.SendAsync(Pack(DanmuType.EnterRoom, string.Format("{{\"roomid\":{0},\"uid\":{1}}}", roomId, _random.Next())));
			}
			catch (Exception ex) {
				throw new ApiException(ex);
			}
		}

		/// <summary>
		/// 发送心跳
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task SendHeartBeatAsync(TcpClient client) {
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			try {
				await client.SendAsync(_heartBeatPacket);
			}
			catch (Exception ex) {
				throw new ApiException(ex);
			}
		}

		/// <summary>
		/// 读取弹幕
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task<Danmu> ReadDanmuAsync(TcpClient client) {
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			byte[] buffer;
			int length;
			DanmuType action;

			buffer = new byte[16];
			await client.ReceiveExactlyAsync(buffer);
			// read header
			ResolveDanmuHeader(buffer, out length, out action);
			buffer = new byte[length];
			if (buffer.Length != 0)
				await client.ReceiveExactlyAsync(buffer);
			// read payload
			return ResolveDanmuPayload(action, buffer);
		}

		private static unsafe void ResolveDanmuHeader(byte[] header, out int length, out DanmuType action) {
			fixed (byte* p = header) {
				int value;

				value = IPAddress.NetworkToHostOrder(*(int*)p);
				// packet length
				length = value - 16;
#pragma warning disable IDE0059
				value = IPAddress.NetworkToHostOrder(*(short*)(p + 4));
				// header length
				value = IPAddress.NetworkToHostOrder(*(short*)(p + 6));
				// version
				value = IPAddress.NetworkToHostOrder(*(int*)(p + 8));
				// action
				action = (DanmuType)value;
				value = IPAddress.NetworkToHostOrder(*(int*)(p + 12));
				// magic
#pragma warning restore IDE0059
			}
		}

		private static Danmu ResolveDanmuPayload(DanmuType action, byte[] payload) {
			switch (action) {
			case DanmuType.Command:
			case DanmuType.Handshaking:
				return new Danmu(action, payload);
			default:
				return Danmu.Empty;
			}
		}

		private static byte[] Pack(DanmuType action, string payload) {
			return Pack(action, payload == null ? null : Encoding.UTF8.GetBytes(payload));
		}

		private static byte[] Pack(DanmuType action, byte[] payload) {
			byte[] packet;

			if (payload == null)
				payload = Array.Empty<byte>();
			packet = new byte[payload.Length + 16];
			using (MemoryStream stream = new MemoryStream(packet)) {
				stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(packet.Length)), 0, 4);
				// packet length
				stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)16)), 0, 2);
				// header length
				stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)1)), 0, 2);
				// version
				stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)action)), 0, 4);
				// action
				stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(1)), 0, 4);
				// magic
				if (payload.Length > 0)
					stream.Write(payload, 0, payload.Length);
				// payload
			}
			return packet;
		}
	}
}
