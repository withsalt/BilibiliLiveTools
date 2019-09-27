using System;

namespace Bilibili.Api {
	/// <summary>
	/// 调用API导致的错误
	/// </summary>
	public sealed class ApiException : Exception {
		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="innerException"></param>
		public ApiException(Exception innerException) : base(string.Empty, innerException) {
		}
	}
}
