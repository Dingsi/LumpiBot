using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LumpiBot.Logging
{
    public class Logger
    {
        public bool Initialized { get; private set; }
        public Dictionary<LogSeverity, Tuple<ConsoleColor, string>> LogTypeInfo { get; }

        readonly BlockingCollection<Tuple<LogSeverity, string, string>> logQueue;

        LogSeverity logLevel;

        internal void SetLevel(LogSeverity logSeverity)
        {
            logLevel = logSeverity;
        }

        public Logger()
        {
            LogTypeInfo = new Dictionary<LogSeverity, Tuple<ConsoleColor, string>>
            {
                { LogSeverity.Verbose,    Tuple.Create(ConsoleColor.White, "") },
                { LogSeverity.Critical, Tuple.Create(ConsoleColor.Red, " Critical ") },
                { LogSeverity.Info,    Tuple.Create(ConsoleColor.DarkGreen, " Info    ") },
                { LogSeverity.Warning, Tuple.Create(ConsoleColor.Yellow, " Warning ") },
                { LogSeverity.Error,   Tuple.Create(ConsoleColor.Red, " Error   ") },
                { LogSeverity.Debug,   Tuple.Create(ConsoleColor.Gray, " Debug   ") },
            };

            logQueue = new BlockingCollection<Tuple<LogSeverity, string, string>>();
        }

        public void Initialize(LogSeverity logTypes, LogFile logFile = null)
        {
            Console.CancelKeyPress += (o, e) => e.Cancel = true;
            Console.OutputEncoding = Encoding.UTF8;

            logLevel = logTypes;

            var logThread = new Thread(async () =>
            {
                while (true)
                {
                    Thread.Sleep(1);

                    Tuple<LogSeverity, string, string> log;

                    if (!logQueue.TryTake(out log))
                        continue;

                    Console.ForegroundColor = ConsoleColor.White;

                    Console.Write($"{log.Item2} |");

                    Console.ForegroundColor = LogTypeInfo[log.Item1].Item1;
                    Console.Write(LogTypeInfo[log.Item1].Item2);
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine($"| {log.Item3}");

                    if (logFile != null)
                        await logFile.WriteAsync($"{log.Item2} |{LogTypeInfo[log.Item1].Item2}| {log.Item3}");

                }
            });

            logThread.IsBackground = true;
            logThread.Start();

            Initialized = true;
        }

        public void Message(LogSeverity logType, string text)
        {
            SetLogger(logType, text);
        }

        public void NewLine()
        {
            logQueue.Add(Tuple.Create(LogSeverity.Info, "", ""));
        }

        public void WaitForKey()
        {
            Console.ReadKey(true);
        }

        public void Clear() => Console.Clear();

        void SetLogger(LogSeverity type, string text)
        {
            if (type <= logLevel)
            {
                logQueue.Add(Tuple.Create(type, DateTime.Now.ToString("T"), text));
            }
        }
    }
}
