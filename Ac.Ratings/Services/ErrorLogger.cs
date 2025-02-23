using System.IO;

namespace Ac.Ratings.Services {
    public static class ErrorLogger {
        public static void LogError(string category, Exception ex) {
            try {
                string logFilePath = Path.Combine(ConfigManager.ErrorLogFilepath);
                if (!File.Exists(logFilePath)) {
                    File.Create(logFilePath);
                }
                string logMessage = $"{DateTime.Now} [{category}] {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";

                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception logEx) {
                Console.WriteLine($"Failed to write error log: {logEx.Message}");
            }
        }
    }
}
