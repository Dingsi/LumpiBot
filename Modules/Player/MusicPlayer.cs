using Discord;
using Discord.Audio;
using LumpiBot.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;

namespace LumpiBot.Modules.Player
{
    public class MusicPlayer
    {
        public static IMessageChannel textChannel;
        public static IAudioClient audioClient;

        public static Track currentTrack;
        public static float Volume = 30;
        public static int SkipSeconds = 0;

        public static ConcurrentQueue<Track> musicQueue = new ConcurrentQueue<Track>();
        public static bool isQueueRunning = false;

        public async Task RunQueueAsync()
        {
            if (isQueueRunning) return;
            isQueueRunning = true;

            await Task.Run(async () =>
            {
                try
                {
                    while (isQueueRunning)
                    {
                        try
                        {
                            Track newTrack;
                            if (musicQueue.TryDequeue(out newTrack))
                            {
                                await textChannel.SendMessageAsync(string.Format("Playing Song '{0}'.", newTrack.Title));
                                await DoPlay(newTrack).ConfigureAwait(true);
                            }
                        }
                        finally
                        {
                            await Task.Delay(50).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Message(LogSeverity.Error, string.Format("Music Queue Error: {0}", ex.Message));
                }
            }).ConfigureAwait(true);
        }

        public async Task NextAsync()
        {
            currentTrack.isCancelRequested = true;
            await textChannel.SendMessageAsync("Playing Next Song...");
        }

        public async Task PlayAsync(string Url, IVoiceChannel PlaybackChannel, IMessageChannel TextChannel)
        {
            var videos = YouTube.Default.GetAllVideos(Url).Where(v => v.AdaptiveKind == AdaptiveKind.Audio);
            var video = videos
                    .Where(v => v.AudioBitrate < 256)
                    .OrderByDescending(v => v.AudioBitrate)
                    .FirstOrDefault();

            var m = Regex.Match(Url, @"\?t=(?<t>\d*)");
            if (m.Captures.Count > 0)
                int.TryParse(m.Groups["t"].ToString(), out SkipSeconds);

            textChannel = TextChannel;

            if (!isQueueRunning)
            {
                try { await audioClient.StopAsync().ConfigureAwait(false); } catch { }
                audioClient = await PlaybackChannel.ConnectAsync();
            }

            if (audioClient != null)
            {
                await textChannel.SendMessageAsync(string.Format("Song '{0}' enqueued.", video.Title));

                Track newTrack = new Track() { Url = await video.GetUriAsync(), Title = video.Title, isCancelRequested = false };
                musicQueue.Enqueue(newTrack);

                await RunQueueAsync();
            }
            else
            {
                Log.Message(LogSeverity.Error, "Unable to Start Audio Client.");
            }

            Log.Message(LogSeverity.Debug, string.Format("Playing {0} - Skipping {1} Sec.", video.Title, SkipSeconds));
        }

        private async Task DoPlay(Track newTrack)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-ss {SkipSeconds} -i \"{newTrack.Url}\" -f s16le -ar 48000 -vn -ac 2 pipe:1 -loglevel quiet",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                CreateNoWindow = true,
            });

            currentTrack = newTrack;

            // Buffering 1 Sec...
            await Task.Delay(1000);

            var outStream = audioClient.CreatePCMStream(AudioApplication.Music);

            int blockSize = 3840;
            byte[] buffer = new byte[blockSize];
            int byteCount;

            while (!p.HasExited)
            {
                byteCount = p.StandardOutput.BaseStream
                        .Read(buffer, 0, blockSize);

                if (newTrack.isCancelRequested)
                {
                    await textChannel.SendMessageAsync(string.Format("Song cancelled."));
                    try
                    {
                        p.Kill();
                    }
                    catch { }
                    break;
                }

                if (byteCount == 0)
                {
                    await textChannel.SendMessageAsync(string.Format("Song ended."));
                    break;
                }
                buffer = AdjustVolume(buffer, (Volume / 100));
                outStream.Write(buffer, 0, byteCount);
            }
            Log.Message(LogSeverity.Debug, "FFMpeg stopped.");
        }

        public async Task SetVolumeAsync(float newVolume)
        {
            if (newVolume >= 0 && newVolume <= 100)
            {
                Volume = newVolume;
                await textChannel.SendMessageAsync(string.Format("Volume set to {0}%", Volume));
            }
            else
            {
                await textChannel.SendMessageAsync("Volume must between 0 and 100.");
            }
        }

        private unsafe byte[] AdjustVolume(byte[] audioSamples, float volume)
        {
            if (Math.Abs(volume - 1f) < 0.0001f) return audioSamples;

            // 16-bit precision for the multiplication
            var volumeFixed = (int)Math.Round(volume * 65536d);

            var count = audioSamples.Length / 2;

            fixed (byte* srcBytes = audioSamples)
            {
                var src = (short*)srcBytes;

                for (var i = count; i != 0; i--, src++)
                    *src = (short)(((*src) * volumeFixed) >> 16);
            }

            return audioSamples;
        }

        public async Task StopAsync()
        {
            isQueueRunning = false;
            currentTrack.isCancelRequested = true;
            musicQueue = new ConcurrentQueue<Track>();
        }
    }

    public class Track
    {
        public string Url;
        public string Title;
        public bool isCancelRequested;
    }
}
