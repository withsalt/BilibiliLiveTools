using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bilibili {
	/// <summary />
	public static class LimitedConcurrencyLevelUtils {
		private const int DEFAULT_MAXDEGREEOFPARALLELISM = 64;

		private static readonly Dictionary<int, TaskScheduler> _taskSchedulers = new Dictionary<int, TaskScheduler>();
		private static readonly Dictionary<int, TaskFactory> _taskFactorys = new Dictionary<int, TaskFactory>();
		private static readonly Dictionary<int, ParallelOptions> _parallelOptionses = new Dictionary<int, ParallelOptions>();
		private static readonly object _syncRoot = new object();

		/// <summary />
		public static TaskScheduler DefaultTaskScheduler => _taskSchedulers[DEFAULT_MAXDEGREEOFPARALLELISM];

		/// <summary />
		public static TaskFactory DefaultTaskFactory => _taskFactorys[DEFAULT_MAXDEGREEOFPARALLELISM];

		/// <summary />
		public static ParallelOptions DefaultParallelOptions => _parallelOptionses[DEFAULT_MAXDEGREEOFPARALLELISM];

		static LimitedConcurrencyLevelUtils() {
			EnsureInstantiated(DEFAULT_MAXDEGREEOFPARALLELISM);
		}

		/// <summary>
		/// 通过指定的最大并发量获取 <see cref="TaskScheduler"/> 的实例
		/// </summary>
		/// <param name="maxDegreeOfParallelism">最大并发量</param>
		/// <returns></returns>
		public static TaskScheduler GetTaskScheduler(int maxDegreeOfParallelism) {
			EnsureInstantiated(maxDegreeOfParallelism);
			return _taskSchedulers[maxDegreeOfParallelism];
		}

		/// <summary>
		/// 通过指定的最大并发量获取 <see cref="TaskFactory"/> 的实例
		/// </summary>
		/// <param name="maxDegreeOfParallelism">最大并发量</param>
		/// <returns></returns>
		public static TaskFactory GetTaskFactory(int maxDegreeOfParallelism) {
			EnsureInstantiated(maxDegreeOfParallelism);
			return _taskFactorys[maxDegreeOfParallelism];
		}

		/// <summary>
		/// 通过指定的最大并发量获取 <see cref="ParallelOptions"/> 的实例
		/// </summary>
		/// <param name="maxDegreeOfParallelism">最大并发量</param>
		/// <returns></returns>
		public static ParallelOptions GetParallelOptions(int maxDegreeOfParallelism) {
			EnsureInstantiated(maxDegreeOfParallelism);
			return _parallelOptionses[maxDegreeOfParallelism];
		}

		private static void EnsureInstantiated(int maxDegreeOfParallelism) {
			if (_taskSchedulers[maxDegreeOfParallelism] != null)
				// 减少lock（Monitor.Enter）造成的性能损失
				return;
			lock (_syncRoot) {
				if (_taskSchedulers[maxDegreeOfParallelism] != null)
					// 再次确认当前最大并发量对应的实例未被创建
					return;
				_taskSchedulers[maxDegreeOfParallelism] = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
				_taskFactorys[maxDegreeOfParallelism] = new TaskFactory(_taskSchedulers[maxDegreeOfParallelism]);
				_parallelOptionses[maxDegreeOfParallelism] = new ParallelOptions {
					CancellationToken = CancellationToken.None,
					MaxDegreeOfParallelism = -1,
					TaskScheduler = _taskSchedulers[maxDegreeOfParallelism]
				};
			}
		}
	}
}
