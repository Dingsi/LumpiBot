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
    public class GoogleSpeech : ModuleBase
    {
        [Command("say", RunMode = RunMode.Async)]
        [Summary("Google TTS")]
        public async Task Say(params string[] text)
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }

            var voiceChannel = ((IGuildUser)Context.User).VoiceChannel;
            IAudioClient audioClient = null;

            try { await audioClient.StopAsync().ConfigureAwait(false); } catch { }
            audioClient = await voiceChannel.ConnectAsync();

            var msg = string.Join(" ", text).ToLower();
            msg = msg.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss").Replace("´", "");
            string url = "http://translate.google.com/translate_tts?tl=de&q=" + msg + "&client=tw-ob";

            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{url}\" -f s16le -ar 48000 -vn -ac 2 pipe:1 -loglevel quiet",
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
        }
    }
}
