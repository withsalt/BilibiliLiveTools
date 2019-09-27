using System;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.Api;
using Bilibili.Settings;

namespace Bilibili {
	/// <summary>
	/// 方法调用助手
	/// </summary>
	public static class SafeCaller {
		/// <summary>
		/// 调用API
		/// </summary>
		/// <param name="action"></param>
		/// <param name="throwOtherException">遇到非 <see cref="ApiException"/> 是否抛出</param>
		/// <param name="maxRetryCount">最大重试次数，-1为无限重试</param>
		/// <param name="retryDelay">重试时延时多少毫秒</param>
		/// <returns></returns>
		public static bool Call(Action action, bool throwOtherException, int maxRetryCount = 3, int retryDelay = 500) {
			if (maxRetryCount < -1)
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount));

			int retryCount;
			bool isSucceed;

			retryCount = 0;
			isSucceed = false;
			while (retryCount < maxRetryCount || maxRetryCount == -1)
				try {
					action();
					isSucceed = true;
					break;
				}
				catch (ApiException apiException) {
					GlobalSettings.Logger.LogException(apiException);
					Thread.Sleep(retryDelay);
					retryCount++;
					continue;
				}
				catch (Exception otherException) {
					GlobalSettings.Logger.LogException(otherException);
					if (throwOtherException)
						throw otherException;
					else
						return false;
				}
			return isSucceed;
		}

		/// <summary>
		/// 调用API
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="func"></param>
		/// <param name="throwOtherException">遇到非 <see cref="ApiException"/> 是否抛出</param>
		/// <param name="maxRetryCount">最大重试次数，-1为无限重试</param>
		/// <param name="retryDelay">重试时延时多少毫秒</param>
		/// <returns></returns>
		public static (TResult, bool) Call<TResult>(Func<TResult> func, bool throwOtherException, int maxRetryCount = 3, int retryDelay = 500) {
			if (maxRetryCount < -1)
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount));

			int retryCount;
			TResult result;
			bool isSucceed;

			retryCount = 0;
			result = default;
			isSucceed = false;
			while (retryCount < maxRetryCount || maxRetryCount == -1)
				try {
					result = func();
					isSucceed = true;
					break;
				}
				catch (ApiException apiException) {
					GlobalSettings.Logger.LogException(apiException);
					Thread.Sleep(retryDelay);
					retryCount++;
					continue;
				}
				catch (Exception otherException) {
					GlobalSettings.Logger.LogException(otherException);
					if (throwOtherException)
						throw otherException;
					else
						return (default, false);
				}
			return (result, isSucceed);
		}

		/// <summary>
		/// 调用API
		/// </summary>
		/// <param name="action"></param>
		/// <param name="throwOtherException">遇到非 <see cref="ApiException"/> 是否抛出</param>
		/// <param name="maxRetryCount">最大重试次数，-1为无限重试</param>
		/// <param name="retryDelay">重试时延时多少毫秒</param>
		/// <returns></returns>
		public static async Task<bool> CallAsync(Func<Task> action, bool throwOtherException, int maxRetryCount = 3, int retryDelay = 500) {
			if (maxRetryCount < -1)
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount));

			int retryCount;
			bool isSucceed;

			retryCount = 0;
			isSucceed = false;
			while (retryCount < maxRetryCount || maxRetryCount == -1)
				try {
					await action();
					isSucceed = true;
					break;
				}
				catch (ApiException apiException) {
					GlobalSettings.Logger.LogException(apiException);
					Thread.Sleep(retryDelay);
					retryCount++;
					continue;
				}
				catch (Exception otherException) {
					GlobalSettings.Logger.LogException(otherException);
					if (throwOtherException)
						throw otherException;
					else
						return false;
				}
			return isSucceed;
		}

		/// <summary>
		/// 调用API
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="func"></param>
		/// <param name="throwOtherException">遇到非 <see cref="ApiException"/> 是否抛出</param>
		/// <param name="maxRetryCount">最大重试次数，-1为无限重试</param>
		/// <param name="retryDelay">重试时延时多少毫秒</param>
		/// <returns></returns>
		public static async Task<(TResult, bool)> CallAsync<TResult>(Func<Task<TResult>> func, bool throwOtherException, int maxRetryCount = 3, int retryDelay = 500) {
			if (maxRetryCount < -1)
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount));

			int retryCount;
			TResult result;
			bool isSucceed;

			retryCount = 0;
			result = default;
			isSucceed = false;
			while (retryCount < maxRetryCount || maxRetryCount == -1)
				try {
					result = await func();
					isSucceed = true;
					break;
				}
				catch (ApiException apiException) {
					GlobalSettings.Logger.LogException(apiException);
					Thread.Sleep(retryDelay);
					retryCount++;
					continue;
				}
				catch (Exception otherException) {
					GlobalSettings.Logger.LogException(otherException);
					if (throwOtherException)
						throw otherException;
					else
						return (default, false);
				}
			return (result, isSucceed);
		}

		/// <summary>
		/// 循环调用API
		/// </summary>
		/// <param name="action"></param>
		/// <param name="delay">延时多少毫秒开始下一次调用</param>
		/// <returns></returns>
		public static void Loop(Action action, int delay = 0) {
			while (true)
				try {
					action();
					Thread.Sleep(delay);
				}
				catch (Exception ex) {
					GlobalSettings.Logger.LogException(ex);
				}
		}
	}
}
