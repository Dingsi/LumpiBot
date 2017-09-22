using Discord;
using LumpiBot.Helpers;
using LumpiBot.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LumpiBot.Modules.Player
{
    class MusicBuffer : Stream
    {
        public MusicBuffer(Track currentTrack, string basename, int maxFileSize)
        {
            CurrentTrack = currentTrack;
            Basename = basename;
            MaxFileSize = maxFileSize;
            CurrentFileStream = new FileStream(this.GetNextFile(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.Write);
        }

        Track CurrentTrack { get; }

        private string Basename { get; }

        private int MaxFileSize { get; } = 2.MiB();

        private long FileNumber = -1;

        private long NextFileToRead = 0;

        public bool BufferingCompleted { get; private set; } = false;

        private ulong CurrentBufferSize = 0;

        private FileStream CurrentFileStream;

        public Task BufferSong(CancellationToken cancelToken) =>
           Task.Run(async () =>
           {
               Process p = null;
               FileStream outStream = null;
               try
               {
                   p = Process.Start(new ProcessStartInfo
                   {
                       FileName = "ffmpeg",
                       Arguments = $"-ss {CurrentTrack.SkipTo} -i {CurrentTrack.SourceVideo.GetUri()} -f s16le -ar 48000 -vn -ac 2 pipe:1 -loglevel quiet",
                       UseShellExecute = false,
                       RedirectStandardOutput = true,
                       RedirectStandardError = false,
                       CreateNoWindow = true,
                   });

                   byte[] buffer = new byte[81920];
                   int currentFileSize = 0;
                   ulong prebufferSize = 100ul.MiB();

                   outStream = new FileStream(Basename + "-" + ++FileNumber, FileMode.Append, FileAccess.Write, FileShare.Read);
                   while (!p.HasExited) //Also fix low bandwidth
                   {
                       int bytesRead = await p.StandardOutput.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancelToken).ConfigureAwait(false);
                       if (currentFileSize >= MaxFileSize)
                       {
                           try
                           {
                               outStream.Dispose();
                           }
                           catch { }
                           outStream = new FileStream(Basename + "-" + ++FileNumber, FileMode.Append, FileAccess.Write, FileShare.Read);
                           currentFileSize = bytesRead;
                       }
                       else
                       {
                           currentFileSize += bytesRead;
                       }
                       CurrentBufferSize += Convert.ToUInt64(bytesRead);
                       await outStream.WriteAsync(buffer, 0, bytesRead, cancelToken).ConfigureAwait(false);
                       while (CurrentBufferSize > prebufferSize)
                           await Task.Delay(100, cancelToken);
                   }
                   BufferingCompleted = true;
               }
               catch (Win32Exception ex)
               {
                   Log.Message(LogSeverity.Error, ex.Message);
                   Log.Message(LogSeverity.Error, ex.StackTrace);
               }
               catch (Exception ex)
               {
                   Log.Message(LogSeverity.Error, $"Buffering stopped: {ex.Message}");
               }
               finally
               {
                   if (outStream != null)
                       outStream.Dispose();
                   Log.Message(LogSeverity.Debug, $"Buffering done.");
                   if (p != null)
                   {
                       try
                       {
                           p.Kill();
                       }
                       catch { }
                       p.Dispose();
                   }
               }
           });

        /// <summary>
        /// Return the next file to read, and delete the old one
        /// </summary>
        /// <returns>Name of the file to read</returns>
        private string GetNextFile()
        {
            string filename = Basename + "-" + NextFileToRead;
            // Log.Message(LogSeverity.Debug, $"MusicBuffer.cs: Require File \"{filename}\"");

            if (NextFileToRead != 0)
            {
                try
                {
                    CurrentBufferSize -= Convert.ToUInt64(new FileInfo(Basename + "-" + (NextFileToRead - 1)).Length);
                    File.Delete(Basename + "-" + (NextFileToRead - 1));
                }
                catch { }
            }
            NextFileToRead++;
            return filename;
        }

        private bool IsNextFileReady()
        {
            return NextFileToRead <= FileNumber;
        }

        private void CleanFiles()
        {
            for (long i = NextFileToRead - 1; i <= FileNumber; i++)
            {
                try
                {
                    File.Delete(Basename + "-" + i);
                }
                catch { }
            }
        }

        //Stream part

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => (long)CurrentBufferSize;

        public override long Position { get; set; } = 0;

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = CurrentFileStream.Read(buffer, offset, count);
            if (read < count)
            {
                if (!BufferingCompleted || IsNextFileReady())
                {
                    CurrentFileStream.Dispose();
                    CurrentFileStream = new FileStream(GetNextFile(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.Write);
                    read += CurrentFileStream.Read(buffer, read + offset, count - read);
                }
                if (read < count)
                    Array.Clear(buffer, read, count - read);
            }
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public new void Dispose()
        {
            CurrentFileStream.Dispose();
            CurrentTrack.CancelTokenSource.Cancel();
            CleanFiles();
            base.Dispose();
        }
    }
}