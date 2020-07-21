using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WatchPartyBot.Modules
{


    // for commands to be available, and have the Context passed to them, we must inherit ModuleBase
    public class BotCommands : ModuleBase
    {
        public struct RoleIdName
        {
            public ulong Id;
            public string Role;
            public RoleIdName(ulong id, string role)
            {
                Id = id;
                Role = role;
            }
        }
        //RoleId List
        static readonly List<RoleIdName> RoleIDs = new List<RoleIdName>() {
            new RoleIdName(111,"ExploitDev"),
            new RoleIdName(222,"CTFTime"),
            new RoleIdName(333,"Cyber101")
        };

        // The following example only requires the user to either have the
        // Administrator permission in this guild or own the bot application.
        [RequireUserPermission(GuildPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageChannels, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [Summary("Joins a bot to the voice channel specified and replays desktop audio")]
        [Command("partytime", RunMode = RunMode.Async)]
        [Alias("watch", "liveaudio")]
        public async Task AudioStreamCommand([
            Summary("Voice Channel name")]
            IVoiceChannel channel = null,
            [Summary("Number of audio channels, 1 for mono, 2 for stereo (Default)")]
            int nAudioChannels = 2,
            [Summary("Sample rate in hertz, 48000 (Default)")]
            int sampleRate = 48000,
            [Summary("Number of bits per sample, 16 (Default)")]
            int bitsPerSample = 16)
        {
            var connection = await channel.ConnectAsync();
            var dstream = connection.CreatePCMStream(AudioApplication.Mixed);

            using (WasapiCapture soundIn = new WasapiLoopbackCapture())
            {
                //initialize the soundIn instance
                soundIn.Initialize();

                //create a SoundSource around the the soundIn instance
                //this SoundSource will provide data, captured by the soundIn instance
                SoundInSource soundInSource = new SoundInSource(soundIn) { FillWithZeros = false };

                //create a source, that converts the data provided by the
                //soundInSource to any other format
                //in this case the "Fluent"-extension methods are being used
                IWaveSource convertedSource = soundInSource
                    .ChangeSampleRate(sampleRate) // sample rate
                    .ToSampleSource()
                    .ToWaveSource(bitsPerSample); //bits per sample
                //int channels = 2;
                //channels...
                using (convertedSource = nAudioChannels == 1 ? convertedSource.ToMono() : convertedSource.ToStereo())
                {
                    //register an event handler for the DataAvailable event of 
                    //the soundInSource
                    //Important: use the DataAvailable of the SoundInSource
                    //If you use the DataAvailable event of the ISoundIn itself
                    //the data recorded by that event might won't be available at the
                    //soundInSource yet
                    soundInSource.DataAvailable += (s, e) =>
                    {
                        //read data from the converedSource
                        //important: don't use the e.Data here
                        //the e.Data contains the raw data provided by the 
                        //soundInSource which won't have your target format
                        byte[] buffer = new byte[convertedSource.WaveFormat.BytesPerSecond / 2];
                        int read;

                        //keep reading as long as we still get some data
                        //if you're using such a loop, make sure that soundInSource.FillWithZeros is set to false
                        while ((read = convertedSource.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            //write the read data to a file
                            // ReSharper disable once AccessToDisposedClosure
                            dstream.Write(buffer, 0, read);
                        }
                    };

                    //we've set everything we need -> start capturing data
                    soundIn.Start();

                    Console.WriteLine("Capturing started ... press any key to stop.");
                    Console.ReadKey();
                    soundIn.Stop();
                }
            }
        }
        //scan initial users
        //every hour, grab log entries from starttime to endtime now()
        /*
         * grab all users from each talk's waiting room (needs the ability to only allow ONE waiting room)
         * check for last update
         * if last update is one of the oldest 500
         */
        //
        //allow user to change roles to a new waitlist role, removes all others
        [RequireUserPermission(GuildPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageChannels, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [Summary("Opens the room for a specific talk")]
        [Command("openroom", RunMode = RunMode.Async)]
        [Alias("starttalk", "jointalk")]
        public async Task GoToWaitingRoomCommand(
            [Summary("Room to wait in [ExploitDev, CTFIntro, 101]")]
            string userRoom = null)
        {
            //get ID for current user
            var userId = Context.User.Id;
            //get user object from ID
            var user = await Context.Guild.GetUserAsync(userId);
            //enumerate all roles current user has
            foreach(ulong id in user.RoleIds)
            {
                if(RoleIDs.Any(e => (e.Id == id)))
                {
                    IRole rId = Context.Guild.GetRole(id);
                    await user.RemoveRoleAsync(rId);
                }
            }
            //check if role requested is one of the ones allowed
            if (RoleIDs.Any(e => e.Role.Contains(userRoom,StringComparison.InvariantCultureIgnoreCase)))
            {
                IRole rId = Context.Guild.GetRole(RoleIDs.Find(f => f.Role.Contains(userRoom, StringComparison.InvariantCultureIgnoreCase)).Id);
                await user.RemoveRoleAsync(rId);
            }
        }
        //listen for new users entering
        //queueUsers(talk)
        //enum allowed roles
        //move members(role of members to move)
        /*
         * If a member is in a waitroom AND one of the eldest 500, move them to the main talk room
         */
        //add role(channel role)
        //remove role(channel role)
        //closeRoom(name of room)
        //openRoom(name of room)
        [RequireUserPermission(GuildPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageChannels, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [Summary("Opens the room for a specific talk")]
        [Command("openroom", RunMode = RunMode.Async)]
        [Alias("starttalk", "jointalk")]
        public async Task OpenRoomCommand(
            [Summary("Room to open")]
            IRole userRole = null)
        {
            List<IUser> users = new List<IUser>();
            foreach(IUser user in await Context.Guild.GetUsersAsync())
            {
                if(users.Count > 1)
                {
                    //get time user came into the server
                }
                users.Append(user);
            }
        }
    }
}
