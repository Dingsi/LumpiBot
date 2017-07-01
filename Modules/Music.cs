using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using LumpiBot.Logging;

namespace LumpiBot.Modules
{
    public class Music : ModuleBase
    {
        public const string MusicDataPath = "cache/music";

        public Music()
        {
            try { Directory.Delete(MusicDataPath, true); } catch { }
            Directory.CreateDirectory(MusicDataPath);

            LumpiBot.Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
        }

        [Command("play"), Summary("Plays the Music from the given Url."), Alias("p")]
        public Task Play(string url)
        {
            var user = Context.User;
            Log.Message(Discord.LogSeverity.Debug, string.Format("Play triggered by {0}", user.Username));

            // TODO: Play Audio from Url

            return Task.CompletedTask;
        }

        [Command("stop"), Summary("Stops the Playback.")]
        public Task Stop()
        {
            var user = Context.User;
            Log.Message(Discord.LogSeverity.Debug, string.Format("Stop Playback triggered by {0}", user.Username));

            // TODO: Stop Playback

            return Task.CompletedTask;
        }

        [Command("pause"), Summary("Paused the Playback.")]
        public Task Pause()
        {
            var user = Context.User;
            Log.Message(Discord.LogSeverity.Debug, string.Format("Pause Playback triggered by {0}", user.Username));

            // TODO: Pause Playback

            return Task.CompletedTask;
        }

        [Command("resume"), Summary("Resume the Playback.")]
        public Task Resume()
        {
            var user = Context.User;
            Log.Message(Discord.LogSeverity.Debug, string.Format("Resume Playback triggered by {0}", user.Username));

            // TODO: Resume Playback

            return Task.CompletedTask;
        }

        [Command("volume"), Summary("Set the Volume"), Alias("vol")]
        public Task Volume(int setVolume)
        {
            var user = Context.User;
            Log.Message(Discord.LogSeverity.Debug, string.Format("Set Volume to {0} by {1}", setVolume, user.Username));

            // TODO: Set Volume

            return Task.CompletedTask;
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
