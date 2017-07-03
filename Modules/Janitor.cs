using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumpiBot.Modules
{
    public class Janitor : ModuleBase
    {
        [Command("prune")]
        [Alias("purge", "clear", "cleanup", "c")]
        [Priority(1000)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CleanAsync(int count = 100)
        {
            try { await Context.Message.DeleteAsync(); } catch { }
            await Context.Channel.GetMessagesAsync(count).ForEachAsync(async batch =>
            {
                foreach(var msg in batch)
                {
                    try { await msg.DeleteAsync(); } catch { }
                }
            });
        }
    }
}
