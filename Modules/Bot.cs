using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LumpiBot.Modules
{
    public class Bot : ModuleBase
    {
        [Command("botnick")]
        [Remarks("Sets the Bot Nickname")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task SetBotNick([Remainder]string name)
        {
            try { await Context.Message.DeleteAsync(); } catch { }

            var self = await Context.Guild.GetCurrentUserAsync();
            await self.ModifyAsync(x => x.Nickname = name);

            await ReplyAsync($"I changed my name to **{name}**");
        }
    }
}
