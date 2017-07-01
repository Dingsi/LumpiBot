using Discord;
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
        public static Config Configuration;
        public static LumpiConfig _config;

        public static void Initialize()
        {
            Configuration = Configuration ?? new Config();
            Config.Setup();
        }

        public static void Setup()
        {
            var ConfigFilePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/config.json";
            if (!File.Exists(ConfigFilePath))
            {
                try
                {
                    StreamWriter sw = File.CreateText(ConfigFilePath);
                    _config = new LumpiConfig();
                    string configStr = JsonConvert.SerializeObject(_config, Formatting.Indented);
                    sw.Write(configStr);
                    sw.Flush();
                    sw.Dispose();

                    Console.WriteLine("New config.json created!");
                    Console.WriteLine("- Please update your Token in config.json and start this Bot again!");
                    Console.WriteLine("- Press any Key to Exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                catch { Console.WriteLine("Unable to create config.json, exiting..."); Console.ReadKey(); Environment.Exit(0); }
            }
            else
            {
                try
                {
                    string configStr = File.ReadAllText(ConfigFilePath);
                    _config = JsonConvert.DeserializeObject<LumpiConfig>(configStr);
                }
                catch { Console.WriteLine("Unable to read config.json at {0}, exiting...", ConfigFilePath); Console.ReadKey(); Environment.Exit(0); }
            }
        }

        internal static T Get<T>(string v)
        {
            try
            {
                switch (v)
                {
                    case "LogSeverity":
                        return (T)Convert.ChangeType(_config.LogSeverity, typeof(T));
                    case "TokenType":
                        return (T)Convert.ChangeType(_config.TokenType, typeof(T));
                    case "Token":
                        return (T)Convert.ChangeType(_config.Token, typeof(T));
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
        public string Token = "";
        public TokenType TokenType = TokenType.Bot;
        public LogSeverity LogSeverity = LogSeverity.Debug;
    }
}
