
using Discord;

namespace LumpiBot.Logging
{
    public class Log
    {
        public static Logger Logger;

        public static void Initialize(LogSeverity logSeverity, LogFile logFile = null)
        {
            Logger = Logger ?? new Logger();
            Logger.Initialize(logSeverity, logFile);
        }

        public static void SetLevel(LogSeverity logSeverity)
        {
            Logger.SetLevel(logSeverity);
        }

        public static void Message(LogSeverity logType, string text)
        {
            Logger.Message(logType, text);
        }

        public static void NewLine()
        {
            Logger.NewLine();
        }

        public static void WaitForKey()
        {
            Logger.WaitForKey();
        }

        public static void Clear() => Logger.Clear();
    }
}
