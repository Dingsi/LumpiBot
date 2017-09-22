using Discord;
using Discord.Audio;
using Discord.Commands;
using LumpiBot.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LumpiBot.Modules
{
    public class Speech : ModuleBase
    {
        public static string SpeechCacheFolder = "speech";

        [Command("say", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Speak)]
        [RequireUserPermission(GuildPermission.SendTTSMessages)]
        [Summary("Google TTS")]
        public async Task Say([Remainder] string text)
        {
            if (Music.isPlaying)
            {
                await Context.Message.Channel.SendMessageAsync("❌ Unable to Say something, Music is playing right now...");
                try { await Context.Message.DeleteAsync(); } catch { }
                return;
            }

            if (!Directory.Exists(LumpiBot.CacheFolder + Path.DirectorySeparatorChar + SpeechCacheFolder))
            {
                Directory.CreateDirectory(LumpiBot.CacheFolder + Path.DirectorySeparatorChar + SpeechCacheFolder);
            }

            try { await Context.Message.DeleteAsync(); } catch { }

            var voiceChannel = ((IGuildUser)Context.User).VoiceChannel;
            IAudioClient audioClient = null;

            try { await audioClient.StopAsync().ConfigureAwait(false); } catch { }
            audioClient = await voiceChannel.ConnectAsync();

            var msg = text.ToLower();
            msg = msg.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss").Replace("´", "").Replace(" ", "%20");
            string url = "http://translate.google.com/translate_tts?tl=de&q=" + msg + "&client=tw-ob";

            HttpClient client = new HttpClient();
            byte[] data = await client.GetByteArrayAsync(url);

            File.WriteAllBytes(LumpiBot.CacheFolder + Path.DirectorySeparatorChar + SpeechCacheFolder + Path.DirectorySeparatorChar + "loc.dat", data);

            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{LumpiBot.CacheFolder + Path.DirectorySeparatorChar + SpeechCacheFolder + Path.DirectorySeparatorChar + "loc.dat"}\" -f s16le -ar 48000 -vn -ac 2 pipe:1 -loglevel quiet",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                CreateNoWindow = true,
            });

            var outStream = audioClient.CreatePCMStream(AudioApplication.Music);

            int blockSize = 3840;
            byte[] buffer = new byte[blockSize];
            int byteCount;

            while (true)
            {
                byteCount = p.StandardOutput.BaseStream.Read(buffer, 0, blockSize);

                if (byteCount == 0)
                {
                    break;
                }
                outStream.Write(buffer, 0, byteCount);
            }

            await Task.Delay(5000);
            try { File.Delete(LumpiBot.CacheFolder + Path.DirectorySeparatorChar + SpeechCacheFolder + Path.DirectorySeparatorChar + "loc.dat"); } catch { }
            try { await audioClient.StopAsync(); } catch { }
        }
    }
}
