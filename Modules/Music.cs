using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using LumpiBot.Logging;
using LumpiBot.Modules.Player;
using Discord;

namespace LumpiBot.Modules
{
    public class Music : ModuleBase
    {
        public const string MusicDataPath = "cache/music";
        public static MusicPlayer musicPlayer;

        public Music()
        {
            try { Directory.Delete(MusicDataPath, true); } catch { }
            Directory.CreateDirectory(MusicDataPath);

            musicPlayer = new MusicPlayer();

            LumpiBot.Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Play Music from Youtube")]
        [Alias("p")]
        public async Task PlayAsync([Summary("Youtube URL")] string Url)
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            if (((IGuildUser)Context.User).VoiceChannel != null)
            {
                await musicPlayer.PlayAsync(Url, ((IGuildUser)Context.User).VoiceChannel, Context.Channel);
            }
            else
            {
                await Context.Channel.SendMessageAsync(string.Format("{0}, you need to join an Voice Channel first!", Context.User.Username));
            }
        }

        [Command("stop")]
        [Summary("Stop Playback")]
        public async Task StopAsync()
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            await musicPlayer.StopAsync();
        }

        [Command("next")]
        [Summary("Play next Track")]
        public async Task PauseAsync()
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            await musicPlayer.NextAsync();
        }

        [Command("volume")]
        [Summary("Set Volume of Playback")]
        [Alias("vol")]
        public async Task VolumeAsync([Summary("New Volume")] int NewVolume)
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            await musicPlayer.SetVolumeAsync(NewVolume);
        }

        private Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var usr = user as SocketGuildUser;
            if (usr == null || oldState.VoiceChannel == newState.VoiceChannel)
                return Task.CompletedTask;

            // TODO: Check If bot is moved or channel is empty.

            return Task.CompletedTask;
        }
    }
}
