using Discord;
using Discord.Audio;
using LumpiBot.Logging;
using System;
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
        public static CancellationTokenSource cancelTokenSource;
        public static float Volume = 30;
        public static int SkipSeconds = 0;

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

            if (audioClient != null)
                try { await audioClient.StopAsync().ConfigureAwait(false); } catch { }

            textChannel = TextChannel;
            audioClient = await PlaybackChannel.ConnectAsync();
            cancelTokenSource = new CancellationTokenSource();
            
            if (audioClient != null)
            {
                await textChannel.SendMessageAsync(string.Format("Playing Song '{0}'", video.Title));
                await DoPlay(await video.GetUriAsync().ConfigureAwait(false)).ConfigureAwait(false);
            }
            else
            {
                Log.Message(LogSeverity.Error, "Unable to Start Audio Client.");
            }

            Log.Message(LogSeverity.Debug, string.Format("Playing {0} - Skipping {1} Sec.", video.Title, SkipSeconds));
        }

        private async Task DoPlay(string Uri)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-ss {SkipSeconds} -i \"{Uri}\" -f s16le -ar 48000 -vn -ac 2 pipe:1 -loglevel quiet",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                CreateNoWindow = true,
            });

            Log.Message(LogSeverity.Debug, "Buffering...");
            await Task.Delay(1000);

            var outStream = audioClient.CreatePCMStream(AudioApplication.Music);

            int blockSize = 3840;
            byte[] buffer = new byte[blockSize];
            int byteCount;

            Log.Message(LogSeverity.Debug, "Sending Data...");

            while (!p.HasExited)
            {
                byteCount = p.StandardOutput.BaseStream
                        .Read(buffer, 0, blockSize);

                if (cancelTokenSource.IsCancellationRequested)
                {
                    Log.Message(LogSeverity.Debug, "Playback cancelled.");
                    await textChannel.SendMessageAsync(string.Format("Song cancelled."));
                    break;
                }

                if (byteCount == 0)
                {
                    Log.Message(LogSeverity.Debug, "Playback ended.");
                    await textChannel.SendMessageAsync(string.Format("Playback ended."));
                    break;
                }
                buffer = AdjustVolume(buffer, (Volume / 100));
                await outStream.WriteAsync(buffer, 0, byteCount, cancelTokenSource.Token);
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
            cancelTokenSource.Cancel();
            await textChannel.SendMessageAsync("Playback stopped.");
        }
    }
}
