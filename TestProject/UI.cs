using System;


namespace TestProject
{
    internal class UI
    {
        public static void Clear()
        {
            Console.Clear();
            Reset();
        }

        public static void LogMessage(string message, ConsoleColor messageColor)
        {
            Console.ForegroundColor = messageColor;
            Console.WriteLine(message);
            Reset();
        }

        public static void LogInfo(string message)
        {
            LogMessage(message, ConsoleColor.White);
        }

        public static void LogStatistics(string message)
        {
            LogMessage(message, ConsoleColor.Green);
        }

        public static void LogWarn(string message)
        {
            LogMessage(message, ConsoleColor.Yellow);
        }

        public static void LogError(string message)
        {
            LogMessage(message, ConsoleColor.Red);
        }

        internal static void LogError(Exception ex)
        {
            LogError($"{ex.Message} - Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                LogError(ex.InnerException.Message);
            }
            Reset();
        }

        internal static void Reset()
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}