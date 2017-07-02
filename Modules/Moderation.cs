using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LumpiBot.Modules
{
    public class Moderation : ModuleBase
    {
        [Command("kick")]
        [Remarks("Kick the specified user.")]
        [RequireUserPermission(Discord.GuildPermission.KickMembers)]
        public async Task Kick([Remainder]SocketGuildUser user)
        {
            await ReplyAsync($"Cya {user.Mention} :wave:");
            await user.KickAsync();
        }
    }
}
