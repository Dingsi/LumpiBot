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
        public static LumpiConfig Configuration;

        public static void Initialize()
        {
            Configuration = Configuration ?? new LumpiConfig();
            Configuration.Setup();
        }

        public static T Get<T>(string v)
        {
            try
            {
                switch (v)
                {
                    case "LogSeverity":
                        return (T)Convert.ChangeType(Configuration.LogSeverity, typeof(T));
                    case "TokenType":
                        return (T)Convert.ChangeType(Configuration.TokenType, typeof(T));
                    case "Token":
                        return (T)Convert.ChangeType(Configuration.Token, typeof(T));
                    case "BotPrefix":
                        return (T)Convert.ChangeType(Configuration.BotPrefix, typeof(T));
                    case "GoogleAPIKey":
                        return (T)Convert.ChangeType(Configuration.GoogleAPIKey, typeof(T));
                    default:
                        break;
                }
            }
            catch { }
            return default(T);
        }
    }

    public class LumpiConfig
    {
        public string BotPrefix = "!";
        public string Token = "";
        public TokenType TokenType = TokenType.Bot;
        public LogSeverity LogSeverity = LogSeverity.Debug;
        public string GoogleAPIKey = "";

        public void Setup()
        {
            var ConfigFilePath = "config.json";
            if (!File.Exists(ConfigFilePath))
            {
                try
                {
                    StreamWriter sw = File.CreateText(ConfigFilePath);
                    string configStr = JsonConvert.SerializeObject(this, Formatting.Indented);
                    sw.Write(configStr);
                    sw.Flush();
                    sw.Dispose();

                    Log.Message(LogSeverity.Verbose, string.Format("New config.json created at {0}.", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)));
                    Log.Message(LogSeverity.Verbose, "- Please set your Discord Bot Token in config.json and start this Bot again!");
                    Log.Message(LogSeverity.Verbose, "- Press any Key to Exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                catch { Log.Message(LogSeverity.Error, "Unable to create config.json, exiting..."); Console.ReadKey(); Environment.Exit(0); }
            }
            else
            {
                try
                {
                    string configStr = File.ReadAllText(ConfigFilePath);
                    var Cfg = JsonConvert.DeserializeObject<LumpiConfig>(configStr);

                    this.Token = Cfg.Token;
                    this.TokenType = Cfg.TokenType;
                    this.LogSeverity = Cfg.LogSeverity;
                    this.BotPrefix = Cfg.BotPrefix;
                    this.GoogleAPIKey = Cfg.GoogleAPIKey;
                }
                catch { Log.Message(LogSeverity.Error, string.Format("Unable to read config.json at {0}, exiting...", ConfigFilePath)); Console.ReadKey(); Environment.Exit(0); }
            }
        }
    }
}
