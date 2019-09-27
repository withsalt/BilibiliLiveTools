using System;

namespace Bilibili {
	/// <summary>
	/// 表示一个Logger
	/// </summary>
	public interface ILogger {
		/// <summary>
		/// 换行
		/// </summary>
		void LogNewLine();

		/// <summary>
		/// 记录普通信息
		/// </summary>
		/// <param name="value"></param>
		void LogInfo(string value);

		/// <summary>
		/// 记录警告信息
		/// </summary>
		/// <param name="value"></param>
		void LogWarning(string value);

		/// <summary>
		/// 记录错误信息
		/// </summary>
		/// <param name="value"></param>
		void LogError(string value);

		/// <summary>
		/// 记录异常
		/// </summary>
		/// <param name="value"></param>
		void LogException(Exception value);
	}
}
