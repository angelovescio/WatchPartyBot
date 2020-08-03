using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace WatchPartyBot.Modules
{


    // for commands to be available, and have the Context passed to them, we must inherit ModuleBase
    public class BotCommands : ModuleBase
    {
        /// <summary>
        /// Timer for sending messages
        /// </summary>
        private static System.Threading.Timer Timer;  
        /// <summary>
        /// Change this to allow more people in the room
        /// </summary>
        private const int MAX_ROOM_SIZE = 200;
        /// <summary>
        /// Change this to allow more rooms
        /// </summary>
        private const int ROOM_COUNT = 6;

        /// <summary>
        /// Interval for timer in minutes*seconds*millisecs
        /// </summary>
        private const int TIMER_INTERVAL = 1 * 60 * 1000;

        /// <summary>
        /// Link to the RTV discord for the return trip
        /// </summary>
        private const string RTV_DISCORD_LINK = "https://discord.gg/room";
        
        /// <summary>
        /// Change this to account for the new room names
        /// </summary>
        private readonly string[] Rooms =
            new string[ROOM_COUNT] { "ExploitDev", "CTFIntro", "Cyber101",
                "Cyber202", "Cyber303", "Cyber404" };
        private readonly string[] URLs =
            new string[ROOM_COUNT] { 
                "ExploitDev: 5 minute warning, please move to <link> when session complete",
                "CTFIntro: 5 minute warning, please move to "+RTV_DISCORD_LINK+" when session complete",
                "Cyber101: 5 minute warning, please move to "+RTV_DISCORD_LINK+" when session complete",
                "Cyber202: 5 minute warning, please move to "+RTV_DISCORD_LINK+" when session complete",
                "Cyber303: 5 minute warning, please move to "+RTV_DISCORD_LINK+" when session complete",
                "Cyber404: 5 minute warning, please move to "+RTV_DISCORD_LINK+" when session complete"
            };
        private readonly string[] WaitRooms;
        private readonly string[] MOTBRooms;

        BotCommands()
        {
            WaitRooms = new string[ROOM_COUNT];
            MOTBRooms = new string[ROOM_COUNT];
            for(int i=0;i<ROOM_COUNT;i++)
            {
                WaitRooms[i] = "Wait" + Rooms[i];
                MOTBRooms[i] = "MOTB" + Rooms[i];
            }
            //SetTimer();
        }

        private void SetTimer()
        {
            //Timer = new System.Timers.Timer(30 * 60 * 1000);
            Timer = new System.Threading.Timer(OnTimedEvent,null,TIMER_INTERVAL, TIMER_INTERVAL);
            
        }

        private void OnTimedEvent(object state)
        {

            ShotsFired();
        }

        private async void ShotsFired()
        {
            var chans = await Context.Guild.GetTextChannelsAsync();
            for (int i = 0; i < ROOM_COUNT; i++)
            {
                IMessageChannel room = chans.Where(e => e.Name.Equals(Rooms[i], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                await room.SendMessageAsync(URLs[i]);
            }
        }

        [Summary("Go to the waiting room for a specific talk")]
        [Command("help", RunMode = RunMode.Async)]
        [Alias("?", "what", "how")]
        public async Task GetHelpCommand()
        {
            IGuildUser user = Context.User as IGuildUser;
            //IEnumerable<IRole> roles = await Context.Guild..GetRolesAsync();
            if(user.GuildPermissions.ManageGuild)
            {
                await user.SendMessageAsync(";help - This menu\r\n" +
                    ";setup - Create the rooms and roles\r\n" +
                    ";dispose - Burn the rooms and roles to the ground\r\n" +
                    ";openroom - Open the talks and put people in there\r\n" +
                    ";waitlist - Add yourself to the line to see the talk, you only can be in ONE line");
            }
            else
            {
                await user.SendMessageAsync(";help - This menu\r\n" +
                    ";setup - Create the rooms and roles\r\n" +
                    ";waitlist - Add yourself to the line to see the talk, you only can be in ONE line. " +
                    "Once you see a talk, you cannot see it again, but you can change your mind and get in " +
                    "another talk's line without penalty as long as you aren't trying to change to a talk " +
                    "you have already seen.");
            }
            
        }
        
        [Summary("Go to the waiting room for a specific talk")]
        [Command("waitlist", RunMode = RunMode.Async)]
        [Alias("linecon", "queue")]
        public async Task GoToWaitingRoomCommand(
            [Summary("Room to wait in [ExploitDev, " +
            "CTFIntro, Cyber101, Cyber202, " +
            "Cyber303, Cyber404]")]
            string userRoom = "ExploitDev")
        {
            //get ID for current user
            var userId = Context.User.Id;
            //get user object from ID
            var user = await Context.Guild.GetUserAsync(userId);

            for (int i = 0; i < ROOM_COUNT; i++)
            {
                //Remove the other waiting rooms
                var roles = Context.Guild.Roles.Where(r => r.Name.Equals("Wait"+Rooms[i],StringComparison.InvariantCultureIgnoreCase));
                await user.RemoveRolesAsync(roles);
                //check if role requested is one of the ones allowed
                if (roles.Any(e => e.Name.Contains(userRoom, StringComparison.InvariantCultureIgnoreCase)))
                {
                    IRole rId = roles.FirstOrDefault(f => f.Name.Equals("Wait"+userRoom, StringComparison.InvariantCultureIgnoreCase));
                    IRole rTryMOTBRoom = Context.Guild.Roles.Where(e => e.Name.Equals("MOTB" + Rooms[i],
                                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (rId != null && !user.RoleIds.Any(f=>f == rTryMOTBRoom.Id))
                    {
                        await user.AddRoleAsync(rId);
                    }
                    else
                    {
                        await Context.User.SendMessageAsync("Sorry, but you already saw that talk, please choose another.");
                    }
                }
            }
        }
        
        [RequireUserPermission(GuildPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageChannels, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [Summary("Kick the up to MAX_ROOM_SIZE users from their talks")]
        [Command("boottheherd", RunMode = RunMode.Async)]
        [Alias("boot", "end")]
        public async Task EndSessionTalkCommand()
        {
            Timer.Dispose();
            //get users with the right roles
            var users = await Context.Guild.GetUsersAsync();
            var chans = await Context.Guild.GetChannelsAsync();
            var defaultrole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "@everyone");
            var defaultperms = new OverwritePermissions(
                sendMessages: PermValue.Deny,
                addReactions: PermValue.Deny,
                viewChannel: PermValue.Deny
            );
            var botrole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "RodsFromGod");
            var botperms = new OverwritePermissions(
                sendMessages: PermValue.Allow,
                addReactions: PermValue.Allow,
                viewChannel: PermValue.Allow,
                manageChannel: PermValue.Allow
            );

            for (int i = 0; i < ROOM_COUNT; i++)
            {
                var room = chans.Where(e => e.Name.Equals(Rooms[i],StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                var role = Context.Guild.Roles.Where(r => r.Name
                .Equals(Rooms[i],StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                foreach (var user in users)
                {
                    await user.RemoveRoleAsync(role);
                }
                await room.DeleteAsync();
            }
            //Strip their roles

            for (int j = 0; j < ROOM_COUNT; j++)
            {
                IGuildChannel cRoom = chans.Where(e => e.Name.Contains(Rooms[j],
               StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (cRoom == null)
                {
                    //create the channels
                    ITextChannel tRoom = await Context.Guild.CreateTextChannelAsync(Rooms[j]);
                    if (tRoom != null)
                    {
                        var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == Rooms[j]);
                        var perms = new OverwritePermissions(
                            sendMessages: PermValue.Allow,
                            addReactions: PermValue.Allow,
                            viewChannel: PermValue.Allow
                        );
                        await tRoom.AddPermissionOverwriteAsync(botrole, botperms);
                        await tRoom.AddPermissionOverwriteAsync(defaultrole, defaultperms);
                        await tRoom.AddPermissionOverwriteAsync(role, perms);
                        //Program.ChannelIDs.Add(tExploitDev);
                    }
                }
            }
            //await SetupRolesRoomsCommand();
            //Clear the room
        }
        //set up the roles
        [RequireUserPermission(GuildPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageChannels, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [Summary("Set up initial roles and rooms wait in [ExploitDev, CTFIntro, Cyber101]")]
        [Command("setuproom", RunMode = RunMode.Async)]
        [Alias("initialize", "setup")]
        public async Task SetupRolesRoomsCommand()
        {
            var defaultrole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "@everyone");
            var defaultperms = new OverwritePermissions(
                sendMessages: PermValue.Deny,
                addReactions: PermValue.Deny,
                viewChannel: PermValue.Deny
            );
            var botrole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "RodsFromGod");
            var botperms = new OverwritePermissions(
                sendMessages: PermValue.Allow,
                addReactions: PermValue.Allow,
                viewChannel: PermValue.Allow,
                manageChannel: PermValue.Allow
            );
            var chans = await Context.Guild.GetChannelsAsync();

            for (int i = 0; i < Rooms.Length; i++)
            {
                IGuildChannel cRoom = chans.Where(e => e.Name.Contains(Rooms[i],
                   StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (cRoom == null)
                {
                    //create the channels
                    ITextChannel tRoom = await Context.Guild.CreateTextChannelAsync(Rooms[i]);
                    if (tRoom != null)
                    {
                        //create the roles
                        IRole rTryRoom = Context.Guild.Roles.Where(e => e.Name.Equals(Rooms[i],
                            StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (rTryRoom == null)
                        {
                            IRole rRoom = await Context.Guild.CreateRoleAsync(name: Rooms[i], color: Color.Teal, 
                                isMentionable: false);

                        }
                        var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == Rooms[i]);
                        var perms = new OverwritePermissions(
                            sendMessages: PermValue.Allow,
                            addReactions: PermValue.Allow,
                            viewChannel: PermValue.Allow
                        );
                        await tRoom.AddPermissionOverwriteAsync(botrole, botperms);
                        await tRoom.AddPermissionOverwriteAsync(defaultrole, defaultperms);
                        await tRoom.AddPermissionOverwriteAsync(role, perms);
                        //Program.ChannelIDs.Add(tExploitDev);
                    }
                }
            }
            for (int j = 0; j < WaitRooms.Length; j++)
            {
                IRole rTryWaitRoom = Context.Guild.Roles.Where(e => e.Name.Equals(WaitRooms[j],
                    StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (rTryWaitRoom == null)
                {
                    IRole rWaitRoom = await Context.Guild.CreateRoleAsync(name: WaitRooms[j], 
                        color: Color.Teal, isMentionable: false);
                }
            }
            for (int k = 0; k < MOTBRooms.Length; k++)
            {
                IRole rTryMOTBRoom = Context.Guild.Roles.Where(e => e.Name.Equals(MOTBRooms[k],
                    StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (rTryMOTBRoom == null)
                {
                    IRole rMOTBRoom = await Context.Guild.CreateRoleAsync(name: MOTBRooms[k], 
                        color: Color.Teal, isMentionable: false);

                }
            }
        }
        //tear it all down
        //delete the roles and rooms
        [RequireUserPermission(GuildPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageChannels, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [Summary("Remove initial roles and rooms wait in [ExploitDev, CTFIntro, Cyber101]")]
        [Command("teardown", RunMode = RunMode.Async)]
        [Alias("dispose", "deleteall")]
        public async Task RemoveRolesRoomsCommand()
        {
            for (int i = 0; i < ROOM_COUNT; i++)
            {
                IEnumerable<IRole> rTryRole = Context.Guild.Roles.Where(e => e.Name.Contains(Rooms[i],
                    StringComparison.InvariantCultureIgnoreCase));
                if (rTryRole != null)
                {
                    foreach (var role in rTryRole)
                    {
                        await role.DeleteAsync();
                    }
                }
            }

            var chans = await Context.Guild.GetChannelsAsync();
            for (int k = 0; k < Rooms.Length; k++)
            {
                IEnumerable <IGuildChannel> cRooms = chans.Where(e => e.Name.Contains(Rooms[k],
               StringComparison.InvariantCultureIgnoreCase));
                if (cRooms != null)
                {
                    foreach (var room in cRooms)
                    {
                        await room.DeleteAsync();
                    }
                }
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
            SetTimer();
            IEnumerable<IAuditLogEntry> logs = await Context.Guild.GetAuditLogsAsync(limit: 10000);
            List<IAuditLogEntry>[] logArray = new List<IAuditLogEntry>[Rooms.Length];
            var users = Context.Guild.GetUsersAsync();
            for (int i = 0; i < Rooms.Length; i++)
            {
                logArray[i] = new List<IAuditLogEntry>(MAX_ROOM_SIZE);
                foreach (var log in logs)
                {
                    if (log.CreatedAt.DateTime < DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)))
                    {
                        break;
                    }
                    if (log.Action == ActionType.MemberRoleUpdated)
                    {
                        IAuditLogData data = log.Data;
                        Discord.Rest.MemberRoleAuditLogData memberRoleAuditLogData = 
                            log.Data as Discord.Rest.MemberRoleAuditLogData;
                        IGuildUser user = await Context.Guild.GetUserAsync(memberRoleAuditLogData.Target.Id);
                        var member_audit = memberRoleAuditLogData.Roles.Where(e => e.Name.Equals("Wait" + Rooms[i], StringComparison.InvariantCultureIgnoreCase));
                        if (member_audit.Count() > 0)
                        {

                            if (user.RoleIds.Any(e => e == member_audit.FirstOrDefault().RoleId))
                            {
                                logArray[i].Add(log);
                            }
                        }
                    }
                }
                logArray[i].Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));
            }

            for (int i = 0; i < Rooms.Length; i++)
            {
                IRole rTryRoom = Context.Guild.Roles.Where(e => e.Name.Equals(Rooms[i],
                    StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (rTryRoom != null)
                {
                    //IRole rExploitDev = Context.Guild.GetRole(rTryRoom.Id);
                    foreach (var entry in logArray[i])
                    {
                        Discord.Rest.MemberRoleAuditLogData memberRoleAuditLogData = entry.Data as Discord.Rest.MemberRoleAuditLogData;
                        IGuildUser user = await Context.Guild.GetUserAsync(memberRoleAuditLogData.Target.Id);
                        for (int j = 0; j < ROOM_COUNT; j++)
                        {
                            IRole rTryWaitRoom = Context.Guild.Roles.Where(e => e.Name.Equals("Wait"+Rooms[j],
                                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            IRole rTryMOTBRoom = Context.Guild.Roles.Where(e => e.Name.Equals("MOTB"+Rooms[j],
                                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (rTryWaitRoom != null && rTryMOTBRoom != null)
                            {
                                if (user.RoleIds.Any(e => e == rTryWaitRoom.Id) && !user.RoleIds.Any(e => e == rTryMOTBRoom.Id))
                                {
                                    //MemberRoleEditInfo mi = memberRoleAuditLogData.Roles.FirstOrDefault();
                                    //IRole role = Context.Guild.GetRole(mi.RoleId);
                                    await user.RemoveRoleAsync(rTryWaitRoom);
                                    await user.AddRoleAsync(rTryRoom);
                                    await user.AddRoleAsync(rTryMOTBRoom);
                                    break;

                                }
                            }
                        }
                    }

                }

            }

        }
    }
}
