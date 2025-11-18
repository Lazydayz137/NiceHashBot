using System;
using System.IO;
using System.Threading;

namespace NHB3.Core.Services
{
    /// <summary>
    /// Simple thread-safe logger with file and console output
    /// </summary>
    public class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        private static readonly object _lockObject = new object();
        private readonly string _logDirectory;
        private LogLevel _minLogLevel = LogLevel.Info;

        public static Logger Instance => _instance.Value;

        private Logger()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void SetMinLogLevel(LogLevel level)
        {
            _minLogLevel = level;
        }

        public void Debug(string message, params object[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        public void Info(string message, params object[] args)
        {
            Log(LogLevel.Info, message, args);
        }

        public void Warning(string message, params object[] args)
        {
            Log(LogLevel.Warning, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        public void Error(Exception ex, string message = null, params object[] args)
        {
            var msg = message != null ? string.Format(message, args) : string.Empty;
            Log(LogLevel.Error, $"{msg}\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }

        private void Log(LogLevel level, string message, params object[] args)
        {
            if (level < _minLogLevel) return;

            try
            {
                var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {formattedMessage}";

                lock (_lockObject)
                {
                    // Console output with color
                    var originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = GetConsoleColor(level);
                    Console.WriteLine(logEntry);
                    Console.ForegroundColor = originalColor;

                    // File output
                    var logFile = Path.Combine(_logDirectory, $"nhb_{DateTime.Now:yyyy-MM-dd}.log");
                    File.AppendAllText(logFile, logEntry + Environment.NewLine);

                    // Cleanup old logs (keep last 30 days)
                    CleanupOldLogs();
                }
            }
            catch (Exception ex)
            {
                // Fallback to console if file logging fails
                Console.WriteLine($"[LOGGER ERROR] Failed to write log: {ex.Message}");
            }
        }

        private void CleanupOldLogs()
        {
            try
            {
                var files = Directory.GetFiles(_logDirectory, "nhb_*.log");
                var cutoffDate = DateTime.Now.AddDays(-30);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        private ConsoleColor GetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return ConsoleColor.Gray;
                case LogLevel.Info: return ConsoleColor.White;
                case LogLevel.Warning: return ConsoleColor.Yellow;
                case LogLevel.Error: return ConsoleColor.Red;
                default: return ConsoleColor.White;
            }
        }
    }

    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
}
