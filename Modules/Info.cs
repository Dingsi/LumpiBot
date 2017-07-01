using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LumpiBot.Modules
{
    public class Info : ModuleBase<CommandContext>
    {
        [Command("help")]
        [Summary("Show all available Commands")]
        [Alias("cmd", "cmds", "commands")]
        public async Task ShowCommands()
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch { }

            var returnStr = "";
            int index = 1;
            foreach (var cmd in LumpiBot.CommandService.Commands)
            {
                string parameters = "";
                foreach(var param in cmd.Parameters)
                {
                    parameters += param.Name + ", ";
                }
                if(parameters != string.Empty)
                {
                    parameters = parameters.Trim().TrimEnd(',');
                }
                else
                {
                    parameters = "/";
                }

                string aliases = "";
                foreach (var alias in cmd.Aliases)
                {
                    aliases += alias + ", ";
                }
                if (aliases != string.Empty)
                {
                    aliases = aliases.Trim().TrimEnd(',');
                }
                else
                {
                    aliases = "/";
                }

                returnStr += string.Format("```markdown\n#{0} {1} - {2}\n\t- Aliases: {3}\n\t- Parameters: {4}\n```\n", index, cmd.Name, cmd.Summary, aliases, parameters);
                index++;
            }

            await Context.Channel.SendMessageAsync(returnStr);
        }
    }
}
