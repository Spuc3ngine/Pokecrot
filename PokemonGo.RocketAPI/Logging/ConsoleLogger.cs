using System;

namespace PokemonGo.RocketAPI.Logging
{
	/// <summary>
	/// The ConsoleLogger is a simple logger which writes all logs to the Console.
	/// </summary>
	public class ConsoleLogger : ILogger
	{
		private LogLevel maxLogLevel;

		/// <summary>
		/// To create a ConsoleLogger, we must define a maximum log level.
		/// All levels above won't be logged.
		/// </summary>
		/// <param name="maxLogLevel"></param>
		public ConsoleLogger(LogLevel maxLogLevel)
		{
			this.maxLogLevel = maxLogLevel;
		}

		/// <summary>
		/// Log a specific message by LogLevel. Won't log if the LogLevel is greater than the maxLogLevel set.
		/// </summary>
		/// <param name="message">The message to log. The current time will be prepended.</param>
		/// <param name="level">Optional. Default <see cref="LogLevel.Info"/>.</param>
		public void Write(string message, LogLevel level = LogLevel.Info)
		{
			if (level > maxLogLevel)
				return;

			Console.WriteLine($"[{ DateTime.Now.ToString("HH:mm:ss")}] { message}");
		}
	}
}