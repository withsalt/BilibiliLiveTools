using System;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Bilibili {
	/// <summary>
	/// 弹幕类型
	/// </summary>
	public enum DanmuType {
		/// <summary />
		HeartBeat = 2,

		/// <summary />
		OnlineCount = 3,

		/// <summary />
		Command = 5,

		/// <summary />
		EnterRoom = 7,

		/// <summary />
		Handshaking = 8
	}

	/// <summary>
	/// 弹幕
	/// </summary>
	public sealed class Danmu {
		private readonly DanmuType _type;
		private readonly byte[] _data;
		private string _text;
		private JObject _json;

		private static readonly Danmu _empty = new Danmu();

		/// <summary>
		/// 空弹幕
		/// </summary>
		public static Danmu Empty => _empty;

		/// <summary>
		/// 弹幕类型
		/// </summary>
		public DanmuType Type => _type;

		/// <summary>
		/// 原始数据
		/// </summary>
		public byte[] Data => _data;

		/// <summary>
		/// 文本
		/// </summary>
		public string Text {
			get {
				if (_data == null)
					throw new InvalidOperationException(nameof(Data) + "不能为null");
				if (_text == null)
					_text = Encoding.UTF8.GetString(_data);
				return _text;
			}
		}

		/// <summary>
		/// JSON
		/// </summary>
		public JObject Json {
			get {
				if (_data == null)
					throw new InvalidOperationException(nameof(Data) + "不能为null");
				if (_json == null)
					_json = JObject.Parse(Text);
				return _json;
			}
		}

		private Danmu() {
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="type"></param>
		/// <param name="data"></param>
		public Danmu(DanmuType type, byte[] data) {
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			_type = type;
			_data = data;
		}
	}
}
