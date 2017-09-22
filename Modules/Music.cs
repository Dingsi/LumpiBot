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
        public static bool isPlaying = false;
        public static MusicPlayer musicPlayer;

        public Music()
        {
            musicPlayer = new MusicPlayer();

            LumpiBot.Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Play Music from Youtube")]
        [RequireUserPermission(GuildPermission.Speak)]
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
                await Context.Channel.SendMessageAsync(string.Format("⚠ {0}, you need to join an Voice Channel first!", Context.User.Username));
            }
        }

        [Command("nowplaying")]
        [Summary("Show current Song Informations.")]
        [RequireUserPermission(GuildPermission.Speak)]
        public async Task NowPlayingAsync()
        {
            try
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(string.Format("🎵 Currently Playing: **{0}** *({1} / {2})*", MusicPlayer.CurrentTrack, MusicPlayer.CurrentTime, MusicPlayer.TotalTime));
            }
            catch { }
        }

        [Command("stop")]
        [Summary("Stop Playback")]
        [RequireUserPermission(GuildPermission.Speak)]
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
        [RequireUserPermission(GuildPermission.Speak)]
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
        [RequireUserPermission(GuildPermission.Speak)]
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
        [RequireUserPermission(GuildPermission.Speak)]
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

            // Check If bot is moved or channel is empty.

            return Task.CompletedTask;
        }
    }
}
