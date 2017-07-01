
using Discord;

namespace LumpiBot.Logging
{
    public class Log
    {
        public static Logger Logger;

        public static void Initialize(LogSeverity logTypes, LogFile logFile = null)
        {
            Logger = Logger ?? new Logger();
            Logger.Initialize(logTypes, logFile);
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
