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
        public async Task CleanAsync(int count = 10)
        {
            try { await Context.Message.DeleteAsync(); } catch { }
            var index = 0;
            var delete = new List<IMessage>(count);
            await Context.Channel.GetMessagesAsync().ForEachAsync(async batch =>
            {
                foreach (var msg in batch.OrderByDescending(msg => msg.Timestamp))
                {
                    if (index >= count)
                    {
                        try { await EndCleanAsync(delete); } catch { }
                        return;
                    }
                    delete.Add(msg);
                    index++;
                }
            });
        }

        private async Task EndCleanAsync(IEnumerable<IMessage> messages)
        {
            foreach (var message in messages)
            {
                await message.DeleteAsync();
            }
        }
    }
}
