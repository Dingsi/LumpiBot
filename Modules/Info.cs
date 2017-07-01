using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LumpiBot.Modules
{
    public class Info : ModuleBase
    {
        public Info()
        {

        }

        [Command("help"), Summary("Show all available Commands"), Alias("cmd", "cmds", "commands")]
        public async Task ShowCommands()
        {
            var returnStr = "";
            foreach (var cmd in LumpiBot.CommandService.Commands)
            {
                string parameters = "";
                foreach(var param in cmd.Parameters)
                {
                    parameters += param.Name + ", ";
                }

                if (cmd.Parameters.Count == 0)
                {
                    parameters = "None";
                }

                string aliases = "";
                foreach (var alias in cmd.Aliases)
                {
                    aliases += alias + ", ";
                }

                if (cmd.Aliases.Count == 0)
                {
                    aliases = "None";
                }

                returnStr += string.Format("```{0} - {1}\n\t- Aliases: {2}\n\t- Parameters: {3}```\n", cmd.Name, cmd.Summary, aliases, parameters);
            }

            await Context.Channel.SendMessageAsync(returnStr);
        }
    }
}
