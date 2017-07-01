using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LumpiBot.Logging;
using LumpiBot.Configuration;
using LumpiBot.Modules;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LumpiBot
{
    class LumpiBot
    {
        public static DiscordShardedClient Client { get; private set; }
        public static CommandService CommandService { get; private set; }
        public static IServiceProvider Services { get; private set; }

        public async Task RunAndBlockAsync(params string[] args)
        {
            await RunAsync(args).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        public async Task RunAsync(params string[] args)
        {
            Log.Initialize(LogSeverity.Debug);
            Config.Initialize();

            Log.SetLevel(Config.Get<LogSeverity>("LogSeverity"));

            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                MessageCacheSize = 10,
                LogLevel = Config.Get<LogSeverity>("LogSeverity"),
                ConnectionTimeout = int.MaxValue,
            });

            CommandService = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync
            });

            Services = new ServiceCollection()
                .BuildServiceProvider();

            // Add Command Modules
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());
            
            Client.Log += _client_Log;
            Client.LoggedIn += _client_LoggedIn;
            Client.MessageReceived += _client_MessageReceivedAsync;

            try
            {
                await Client.LoginAsync(Config.Get<TokenType>("TokenType"), Config.Get<string>("Token"));
                await Client.StartAsync();
            }
            catch (Exception ex) { Log.Message(LogSeverity.Error, ex.Message); }
        }

        private async Task _client_MessageReceivedAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            // Determine if the message is a command, based on if it starts with '.' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))) return;
            
            var context = new CommandContext(Client, message);

            var result = await CommandService.ExecuteAsync(context, argPos, Services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private Task _client_Log(LogMessage arg)
        {
            if(arg.Severity <= Config.Get<LogSeverity>("LogSeverity"))
            {
                Log.Message(arg.Severity, arg.Message);
            }
            
            return Task.CompletedTask;
        }

        private Task _client_LoggedIn()
        {
            var TokenType = Client.TokenType;
            Log.Message(LogSeverity.Verbose, string.Format("Logged in as {0}.", TokenType));
            return Task.CompletedTask;
        }
    }
}
