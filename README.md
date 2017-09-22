# LumpiBot

[![N|Solid](http://i.imgur.com/oOI4TlV.png)](http://i.imgur.com/oOI4TlV.png)

# - Currently in Development!!!

### Prerequisites
- Notepad++ (or some other decent text Editor)
- NET.Core 1.1
- FFmpeg ([Win Download](https://ffmpeg.zeranoe.com/builds/))
- Visual Studio (for NuGet Packages)
- LibSodium, LibOpus (found in _libs for Win, Linux)
- An Discord Bot Token and Client Id ([Create your Bot here...](https://discordapp.com/developers/applications/me))
- Invitation Link: https://discordapp.com/oauth2/authorize?client_id=[YOUR_CLIENT_ID]&scope=bot

### Installation (Windows)
- Download LumpiBot via Git.
- Open LumpiBot.sln in Visual Studio to download all missing NuGet Packages.
- Copy opus.dll & libsodium.dll from _libs into the root directory.
- Build Project in Visual Studio or via. 'dotnet build' Command.
- Run LumpiBot for the first Time, it will create an config.json at [Project Folder]/bin/netcoreappX.X/
- Insert your Bot Token:
```json
    "Token": "_INSERT_TOKEN_HERE_",
```
- Save and run your Bot.

### Installation (Linux)
- Download LumpiBot via Git.
- Use Command 'dotnet restore'
- Use Command 'dotnet build'
- Copy opus.so & libsodium.so from _libs into the root directory.
- Run LumpiBot for the first Time, it will create an config.json at [Project Folder]/bin/netcoreappX.X/
- Insert your Bot Token:
```json
    "Token": "_INSERT_TOKEN_HERE_",
```
- Save and run your Bot via. 'dotnet run'.

### Used NuGet Packages

| Package | Url |
| ------ | ------ |
| Discord.Net | https://github.com/RogueException/Discord.Net |
| Discord.Net.Commands | https://github.com/RogueException/Discord.Net |
| Newtonsoft.Json | http://www.newtonsoft.com/json |
| LibVideo | https://www.nuget.org/packages/libvideo.dingsi/1.4.0 |