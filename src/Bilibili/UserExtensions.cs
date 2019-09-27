using System;
using Bilibili.Settings;

namespace Bilibili {
	/// <summary />
	public static class UserExtensions {
		/// <summary />
		public static void LogNewLine(this User user) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			GlobalSettings.Logger.LogNewLine();
		}

		/// <summary />
		public static void LogInfo(this User user, string value) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			GlobalSettings.Logger.LogInfo($"用户\"{user}\"：{value}");
		}

		/// <summary />
		public static void LogWarning(this User user, string value) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			GlobalSettings.Logger.LogWarning($"用户\"{user}\"：{value}");
		}

		/// <summary />
		public static void LogError(this User user, string value) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			GlobalSettings.Logger.LogError($"用户\"{user}\"：{value}");
		}
	}
}
