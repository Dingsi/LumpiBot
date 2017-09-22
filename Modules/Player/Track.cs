using Discord;
using System.Threading;
using VideoLibrary;

namespace LumpiBot.Modules.Player
{
    public class Track
    {
        public string Id;
        public YouTubeVideo SourceVideo;
        
        public int SkipTo = 0;
        public bool isPaused = false;

        public IMessageChannel sourceTextChannel = null;
        public IVoiceChannel sourceVoiceChannel = null;

        public CancellationTokenSource CancelTokenSource;
    }
}
