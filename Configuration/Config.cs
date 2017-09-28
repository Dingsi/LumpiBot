using Discord;
using LumpiBot.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LumpiBot.Configuration
{
    public class Config
    {
        public BotConfiguration Bot { get; private set; }

        public Config()
        {
            var jsonFile = "config.json";
            try
            {
                if (!File.Exists(jsonFile))
                {
                    BotConfiguration newConfig = new BotConfiguration();

                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine(" SETUP");
                    Console.WriteLine("-------------------------------------");

                    Console.WriteLine();

                    Console.WriteLine("Bot Prefix: (Default: !)");
                    string BotPrefix = Console.ReadLine();
                    if(BotPrefix == string.Empty)
                    {
                        BotPrefix = "!";
                    }
                    newConfig.Prefix = BotPrefix;
                    Console.WriteLine();

                    Console.WriteLine("Discord Bot Token:");
                    string BotToken = string.Empty;
                    while (BotToken == string.Empty)
                    {
                        BotToken = Console.ReadLine();
                        if(BotToken == string.Empty)
                        {
                            Console.WriteLine("Bot Token is required!");
                            Console.WriteLine("-> Get your Token from: https://discordapp.com/developers/applications/me");
                        }
                    }
                    newConfig.Token = BotToken;
                    Console.WriteLine();

                    Console.WriteLine("Google API Key:");
                    string GoogleAPIKey = string.Empty;
                    while (GoogleAPIKey == string.Empty)
                    {
                        GoogleAPIKey = Console.ReadLine();
                        if (GoogleAPIKey == string.Empty)
                        {
                            Console.WriteLine("Google API Key is required!");
                            Console.WriteLine("-> Get your Key from: https://developers.google.com/youtube/v3/getting-started");
                        }
                    }
                    newConfig.GoogleAPIKey = GoogleAPIKey;

                    // Default Values
                    newConfig.LogSeverity = LogSeverity.Debug;
                    newConfig.TokenType = TokenType.Bot;

                    File.WriteAllText(jsonFile, JsonConvert.SerializeObject(newConfig, Formatting.Indented));

                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine(" New Configuration File created!");
                    Console.WriteLine("-------------------------------------");
                }

                string jsonStr = File.ReadAllText(jsonFile);
                this.Bot = JsonConvert.DeserializeObject<BotConfiguration>(jsonStr);
            }
            catch { throw new Exception("Invalid config.json"); }
        }
    }

    public class BotConfiguration
    {
        public string Prefix = "!";
        public TokenType TokenType = TokenType.Bot;
        public string Token = "";
        public string GoogleAPIKey = "";
        public LogSeverity LogSeverity = LogSeverity.Debug;
    }
}
