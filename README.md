# WatchPartyBot, a C# / [Discord.Net](https://github.com/discord-net/Discord.Net) Project

Join your favorite stream RIGHT into the Voice Channel using your desktop audio

### Building/Debugging
* When building/debugging this bot it will fail due to a missing config.json file when launched the first time. You'll want to put a config.json file with the following content in **/bin/Debug/netcoreapp2.x/**
```json
{
    "Token": "your_bot_token",
    "Prefix": ";"
}
```
### Bot Control

The bot responds to several commands and aliases which all kick off the streaming to the VC (partytime, watch, liveaudio)

### Bot Installation

You really need to make an application FIRST on Discord following [these instructions](https://discordpy.readthedocs.io/en/latest/discord.html)

Requires the Opus libraries from the people at [Discord.NET](https://discord.foxbot.me/binaries/win64/opus.dll). Drop it in the same directory as your executable.

### Warnings on usage

It uses a loopback to play the audio, so make sure you don't have other audio apps running ASIDE from the ONE THING you want to stream. Especially, do not also have Discord running on the same PC, joined to any voice channel. The loopback is dumb, it will play back ALL desktop audio.# WatchPartyBot
