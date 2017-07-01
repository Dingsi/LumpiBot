using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LumpiBot.Logging;
using LumpiBot.Configuration;

namespace LumpiBot
{
    class LumpiBot
    {
        public DiscordShardedClient _client;
        public async Task RunAndBlockAsync(params string[] args)
        {
            await RunAsync(args).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        public async Task RunAsync(params string[] args)
        {
            this._client = new DiscordShardedClient(new DiscordSocketConfig
            {
                MessageCacheSize = 10,
                LogLevel = LogSeverity.Warning,
                ConnectionTimeout = int.MaxValue,
            });

            Config.Initialize();
            Config.Setup();

            Log.Initialize(Config.Get<LogSeverity>("LogSeverity"));
            
            this._client.Log += _client_Log;
            this._client.LoggedIn += _client_LoggedIn;

            try
            {
                await this._client.LoginAsync(Config.Get<TokenType>("TokenType"), Config.Get<string>("Token"));
                await this._client.StartAsync();
            }
            catch (Exception ex) { Log.Message(LogSeverity.Error, ex.Message); }
        }

        private Task _client_Log(LogMessage arg)
        {
            Log.Message(arg.Severity, arg.Message);
            return Task.CompletedTask;
        }

        private Task _client_LoggedIn()
        {
            var TokenType = this._client.TokenType;
            Log.Message(LogSeverity.Verbose, string.Format("Logged in as {0}.", TokenType));
            return Task.CompletedTask;
        }
    }
}
