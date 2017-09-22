using Discord;
using Discord.Audio;
using LumpiBot.Helpers;
using LumpiBot.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;
using Newtonsoft.Json;
using System.Xml;
using LumpiBot.Configuration;
using System.Web;

namespace LumpiBot.Modules.Player
{
    public class MusicPlayer
    {
        private const int _milliseconds = 20;
        private const int _samplesPerFrame = (48000 / 1000) * _milliseconds;
        private const int _frameBytes = 3840;   //16-bit, 2 channels
        public static string CurrentTrack = string.Empty;
        public static TimeSpan TotalTime = TimeSpan.Zero;
        public static TimeSpan CurrentTime = TimeSpan.Zero;
        private ulong bytesSent { get; set; }

        public static IAudioClient audioClient = null;
        public static ConcurrentQueue<Track> Queue = new ConcurrentQueue<Track>();
        public static bool isQueueRunning = false;

        public static float Volume = 30;

        public static Track playingTrack;

        public static string MusicCacheFolder = "music";

        public async Task PlayAsync(string Url, IVoiceChannel voiceChannel, IMessageChannel txtChannel)
        {
            await LumpiBot.Client.SetGameAsync(string.Empty, null, StreamType.NotStreaming);

            if (!isQueueRunning)
            {
                await RunQueueAsync();
                isQueueRunning = true;
            }

            var YouTubeVideo = GetYouTubeVideo(Url);
            if (YouTubeVideo != null)
            {
                var query = HttpUtility.ParseQueryString(new Uri(Url).Query);
                var videoId = query["v"];

                var Track = new Track() { Id = videoId, SourceVideo = YouTubeVideo, CancelTokenSource = new CancellationTokenSource(), sourceVoiceChannel = voiceChannel, sourceTextChannel = txtChannel };
                Queue.Enqueue(Track);

                var msg = await txtChannel.SendMessageAsync($"▶ \"{Track.SourceVideo.Title}\" enqueued.");
                await Task.Delay(2000);
                await msg.DeleteAsync();
            }
        }

        public async Task SetVolumeAsync(int newVolume)
        {
            if (playingTrack == null)
                return;

            if (newVolume <= 100 && newVolume >= 0)
            {
                Volume = newVolume;
                var msg = await playingTrack.sourceTextChannel.SendMessageAsync($"📶 Volume set to {newVolume}%");
                await Task.Delay(5000);
                await msg.DeleteAsync();
            }
            else
            {
                var msg = await playingTrack.sourceTextChannel.SendMessageAsync("📶 Volume must between 0 and 100.");
                await Task.Delay(5000);
                await msg.DeleteAsync();
            }
        }

        public void Next()
        {
            playingTrack.CancelTokenSource.Cancel();
        }

        public void Stop()
        {
            Queue = new ConcurrentQueue<Track>();
            isQueueRunning = false;
            playingTrack.CancelTokenSource.Cancel();
            try
            {
                TotalTime = TimeSpan.Zero;
                CurrentTime = TimeSpan.Zero;
                Music.isPlaying = false;
                audioClient.StopAsync();
            }
            catch { }
        }

        private Task RunQueueAsync()
        {
            new TaskFactory().StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        Track currentTrack;
                        if (Queue.TryDequeue(out currentTrack))
                        {
                            playingTrack = currentTrack;
                            try { await audioClient.StopAsync().ConfigureAwait(false); } catch { Log.Message(LogSeverity.Warning, "MusicPlayer.cs: Unable to Stop Audio Client"); }
                            audioClient = await playingTrack.sourceVoiceChannel.ConnectAsync();
                            if (audioClient != null)
                            {
                                try { await LumpiBot.Client.SetGameAsync(playingTrack.SourceVideo.Title); } catch { }

                                await StreamAsync(playingTrack).ConfigureAwait(true);
                            }
                        }
                    }
                    finally
                    {
                        await Task.Delay(1000);
                    }
                }
            });
            
            return Task.CompletedTask;
        }

        public async Task LeaveAsync()
        {
            try
            {
                try { await LumpiBot.Client.SetGameAsync(string.Empty); } catch { }
                try { await audioClient.StopAsync().ConfigureAwait(false); } catch { Log.Message(LogSeverity.Warning, "MusicPlayer.cs: Unable to Stop Audio Client"); }
                audioClient = await playingTrack.sourceVoiceChannel.ConnectAsync();
            }
            catch { }
        }

        private async Task StreamAsync(Track currentTrack)
        {
            try
            {
                var response = await new HttpClient().GetStringAsync(string.Format("https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id={0}&key={1}", currentTrack.Id, Config.Configuration.GoogleAPIKey));
                var json = JsonConvert.DeserializeObject<YTRootObject>(response);
                if (json != null)
                {
                    foreach(var o in json.items)
                    {
                        TotalTime = XmlConvert.ToTimeSpan(o.contentDetails.duration);
                    }

                    CurrentTrack = currentTrack.SourceVideo.Title;
                    await currentTrack.sourceTextChannel.SendMessageAsync(string.Format("🎶 Now Playing **{0}** *({1})*", currentTrack.SourceVideo.Title, TotalTime));
                }
            }
            catch (Exception ex) { Log.Message(LogSeverity.Error, ex.Message); }

            Music.isPlaying = true;
            bytesSent = (ulong)currentTrack.SkipTo * 3840 * 50;

            if(!Directory.Exists(LumpiBot.CacheFolder + Path.DirectorySeparatorChar + MusicCacheFolder + Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(LumpiBot.CacheFolder + Path.DirectorySeparatorChar + MusicCacheFolder + Path.DirectorySeparatorChar);
            }

            var filename = LumpiBot.CacheFolder + Path.DirectorySeparatorChar + MusicCacheFolder + Path.DirectorySeparatorChar + DateTime.Now.Ticks.ToString();
            var inStream = new MusicBuffer(currentTrack, filename, _frameBytes * 100);
            var bufferTask = inStream.BufferSong(currentTrack.CancelTokenSource.Token).ConfigureAwait(false);

            try
            {
                var attempt = 0;

                var prebufferingTask = CheckPrebufferingAsync(inStream, currentTrack.CancelTokenSource.Token, 1.MiB()); //Fast connection can do this easy
                var finished = false;
                var count = 0;
                var sw = new Stopwatch();
                var slowconnection = false;
                sw.Start();
                while (!finished)
                {
                    var t = await Task.WhenAny(prebufferingTask, Task.Delay(2000, currentTrack.CancelTokenSource.Token));
                    if (t != prebufferingTask)
                    {
                        count++;
                        if (count == 10)
                        {
                            slowconnection = true;
                            prebufferingTask = CheckPrebufferingAsync(inStream, currentTrack.CancelTokenSource.Token, 20.MiB());
                            Log.Message(LogSeverity.Warning, "Slow connection buffering more to ensure no disruption, consider hosting in cloud");
                            continue;
                        }

                        if (inStream.BufferingCompleted && count == 1)
                        {
                            Log.Message(LogSeverity.Warning, "Prebuffering canceled. Cannot get any data from the stream.");
                            Music.isPlaying = false;
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (prebufferingTask.IsCanceled)
                    {
                        Log.Message(LogSeverity.Warning, "Prebuffering canceled. Cannot get any data from the stream.");
                        Music.isPlaying = false;
                        return;
                    }
                    finished = true;
                }
                sw.Stop();
                Log.Message(LogSeverity.Debug, "Prebuffering successfully completed in " + sw.Elapsed);

                var outStream = audioClient.CreatePCMStream(AudioApplication.Music);

                int nextTime = Environment.TickCount + _milliseconds;

                byte[] buffer = new byte[_frameBytes];
                while (!currentTrack.CancelTokenSource.Token.IsCancellationRequested)
                {
                    var read = await inStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    if (read < _frameBytes)
                        Log.Message(LogSeverity.Debug, $"read {read}");
                    unchecked
                    {
                        bytesSent += (ulong)read;
                    }

                    CurrentTime = TimeSpan.FromSeconds(bytesSent / (float)_frameBytes / (1000 / (float)_milliseconds));

                    if (read < _frameBytes)
                    {
                        if (read == 0)
                        {
                            if (inStream.BufferingCompleted)
                                break;
                            if (attempt++ == 20)
                            {
                                currentTrack.CancelTokenSource.Cancel();
                                break;
                            }
                            if (slowconnection)
                            {
                                Log.Message(LogSeverity.Warning, "Slow connection has disrupted music, waiting a bit for buffer...");

                                await Task.Delay(1000, currentTrack.CancelTokenSource.Token).ConfigureAwait(false);
                                nextTime = Environment.TickCount + _milliseconds;
                            }
                            else
                            {
                                await Task.Delay(100, currentTrack.CancelTokenSource.Token).ConfigureAwait(false);
                                nextTime = Environment.TickCount + _milliseconds;
                            }
                        }
                        else
                            attempt = 0;
                    }
                    else
                        attempt = 0;

                    while (currentTrack.isPaused)
                    {
                        await Task.Delay(200, currentTrack.CancelTokenSource.Token).ConfigureAwait(false);
                        nextTime = Environment.TickCount + _milliseconds;
                    }

                    float calcvol = Volume / 100;
                    buffer = ScaleVolumeSafeAllocateBuffers(buffer, calcvol);
                    if (read != _frameBytes) continue;
                    nextTime = unchecked(nextTime + _milliseconds);
                    int delayMillis = unchecked(nextTime - Environment.TickCount);
                    if (delayMillis > 0)
                        await Task.Delay(delayMillis, currentTrack.CancelTokenSource.Token).ConfigureAwait(false);
                    await outStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                }
            }
            finally
            {
                await bufferTask;
                inStream.Dispose();
                Music.isPlaying = false;
                await LumpiBot.Client.SetGameAsync(string.Empty);
            }
        }

        private async Task CheckPrebufferingAsync(MusicBuffer inStream, CancellationToken cancelToken, long size)
        {
            while (!inStream.BufferingCompleted && inStream.Length < size)
            {
                await Task.Delay(100, cancelToken);
            }
            Log.Message(LogSeverity.Debug, "Buffering successfull.");
        }

        public static byte[] ScaleVolumeSafeAllocateBuffers(byte[] audioSamples, float volume)
        {
            var output = new byte[audioSamples.Length];
            if (Math.Abs(volume - 1f) < 0.0001f)
            {
                Buffer.BlockCopy(audioSamples, 0, output, 0, audioSamples.Length);
                return output;
            }

            // 16-bit precision for the multiplication
            int volumeFixed = (int)Math.Round(volume * 65536d);

            for (var i = 0; i < output.Length; i += 2)
            {
                // The cast to short is necessary to get a sign-extending conversion
                int sample = (short)((audioSamples[i + 1] << 8) | audioSamples[i]);
                int processed = (sample * volumeFixed) >> 16;

                output[i] = (byte)processed;
                output[i + 1] = (byte)(processed >> 8);
            }

            return output;
        }

        private YouTubeVideo GetYouTubeVideo(string Url)
        {
            var videos = YouTube.Default.GetAllVideos(Url).Where(v => v.AdaptiveKind == AdaptiveKind.Audio);
            var video = videos
                    .Where(v => v.AudioBitrate < 256)
                    .OrderByDescending(v => v.AudioBitrate)
                    .FirstOrDefault();

            var m = Regex.Match(Url, @"\?t=(?<t>\d*)");
            var SkipTo = 0;
            if (m.Captures.Count > 0)
                int.TryParse(m.Groups["t"].ToString(), out SkipTo);

            return video;
        }
    }
}
