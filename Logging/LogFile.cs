﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LumpiBot.Logging
{
    public sealed class LogFile : IDisposable
    {
        readonly FileStream logStream;

        public LogFile(string directory, string file)
        {
            logStream = new FileStream($"{directory}/{DateTime.Now:yyyy-MM-dd_hh-mm-ss}_{file}", FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, true);
        }

        public async Task WriteAsync(string logMessage)
        {
            var logBytes = Encoding.Unicode.GetBytes($"{logMessage}\n");

            await logStream.WriteAsync(logBytes, 0, logBytes.Length);
            await logStream.FlushAsync();
        }

        public void Dispose()
        {
            logStream.Dispose();
        }
    }
}
