using System;
using System.IO;

namespace Bilibili.Settings {
	/// <summary>
	/// 全局设置
	/// </summary>
	public static class GlobalSettings {
		private const string BILIBILISETTINGS_PATH = @"Settings\Bilibili.json";

		private static BilibiliSettings _bilibili;
		private static ILogger _logger = DummyLogger.Instance;
		private static readonly object _syncRoot = new object();

		/// <summary />
		public static BilibiliSettings Bilibili => _bilibili;

		/// <summary />
		public static ILogger Logger {
			get => _logger;
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				_logger = value;
			}
		}

		/// <summary>
		/// 加载设置文件
		/// </summary>
		public static void LoadAll() {
			lock (_syncRoot) {
				_bilibili = BilibiliSettings.FromJson(File.ReadAllText(BILIBILISETTINGS_PATH));
			}
		}

		private sealed class DummyLogger : ILogger {
			private static readonly DummyLogger _instance = new DummyLogger();

			public static DummyLogger Instance => _instance;

			private DummyLogger() {
			}

			public void LogNewLine() {
			}

			public void LogInfo(string value) {
			}

			public void LogWarning(string value) {
			}

			public void LogError(string value) {
			}

			public void LogException(Exception value) {
			}
		}
	}
}
