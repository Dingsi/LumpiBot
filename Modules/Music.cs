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
        public static MusicPlayer musicPlayer;

        public Music()
        {
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

        [Command("test", RunMode = RunMode.Async)]
        public async Task PlayTest()
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            if (((IGuildUser)Context.User).VoiceChannel != null)
            {
                await musicPlayer.PlayAsync("https://www.youtube.com/watch?v=gF7gJTliXXo", ((IGuildUser)Context.User).VoiceChannel, Context.Channel);
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
            musicPlayer.Stop();
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
            musicPlayer.Next();
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

        [Command("leave")]
        [Summary("Leaves the current Channel")]
        public async Task leaveAsync()
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }
            await musicPlayer.LeaveAsync();
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
