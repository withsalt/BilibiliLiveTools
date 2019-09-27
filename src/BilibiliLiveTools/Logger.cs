using Bilibili;
using System;
using System.Text;

namespace BilibiliLiveTools {
	internal sealed class Logger : ILogger {
		private static readonly Logger _instance = new Logger();
		private static readonly object _syncRoot = new object();

		public static Logger Instance => _instance;

		private Logger() {
		}

		public void LogNewLine() {
			lock (_syncRoot)
				Console.WriteLine();
		}

		public void LogInfo(string value) {
			lock (_syncRoot)
				Console.WriteLine($"[{DateTime.Now.ToString()}] {value}");
		}

		public void LogWarning(string value) {
			lock (_syncRoot) {
				ConsoleColor color;

				color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"[{DateTime.Now.ToString()}] {value}");
				Console.ForegroundColor = color;
			}
		}

		public void LogError(string value) {
			lock (_syncRoot) {
				ConsoleColor color;

				color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine($"[{DateTime.Now.ToString()}] {value}");
				Console.ForegroundColor = color;
			}
		}

		public void LogException(Exception value) {
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			LogError(ExceptionToString(value));
		}

		private static string ExceptionToString(Exception exception) {
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			StringBuilder sb;

			sb = new StringBuilder();
			DumpException(exception, sb);
			return sb.ToString();
		}

		private static void DumpException(Exception exception, StringBuilder sb) {
			sb.AppendLine("Type: " + Environment.NewLine + exception.GetType().FullName);
			sb.AppendLine("Message: " + Environment.NewLine + exception.Message);
			sb.AppendLine("Source: " + Environment.NewLine + exception.Source);
			sb.AppendLine("StackTrace: " + Environment.NewLine + exception.StackTrace);
			sb.AppendLine("TargetSite: " + Environment.NewLine + exception.TargetSite.ToString());
			sb.AppendLine("----------------------------------------");
			if (exception.InnerException != null)
				DumpException(exception.InnerException, sb);
		}
	}
}
