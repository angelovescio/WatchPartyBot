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

namespace WatchPartyBot.Modules
{


    // for commands to be available, and have the Context passed to them, we must inherit ModuleBase
    public class BotCommands : ModuleBase
    {
        

        //scan initial users
        //every hour, grab log entries from starttime to endtime now()
        /*
         * grab all users from each talk's waiting room (needs the ability to only allow ONE waiting room)
         * check for last update
         * if last update is one of the oldest 500
         */
        //
        //allow user to change roles to a new waitlist role, removes all others
        [Summary("Go to the waiting room for a specific talk")]
        [Command("waitlist", RunMode = RunMode.Async)]
        [Alias("starttalk", "jointalk")]
        public async Task GoToWaitingRoomCommand(
            [Summary("Room to wait in [ExploitDev, CTFIntro, Cyber101]")]
            string userRoom = null)
        {
            //get ID for current user
            var userId = Context.User.Id;
            //get user object from ID
            var user = await Context.Guild.GetUserAsync(userId);
            //enumerate all roles current user has
            IEnumerable<IRole> iRoles = Context.Guild.Roles.Where(e => (Program.WaitlistRoleIDs.Contains(e.Name)));
            await user.RemoveRolesAsync(iRoles);
            //check if role requested is one of the ones allowed
            if (Program.WaitlistRoleIDs.Any(e => e.Contains(userRoom,StringComparison.InvariantCultureIgnoreCase)))
            {
                IRole rId = iRoles.FirstOrDefault(f => f.Name.Contains(userRoom, StringComparison.InvariantCultureIgnoreCase));
                if (rId != null)
                {
                    await user.AddRoleAsync(rId);
                }
            }
        }
        //set up the roles
        [RequireUserPermission(GuildPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageChannels, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [Summary("Move the up to 500 users to their talks")]
        [Command("movetheherd", RunMode = RunMode.Async)]
        [Alias("move", "moo")]
        public async Task StartSessionTalkCommand()
        {
            //get users with the right roles
            //find the time they set their role
            //move them to a room
        }
        [RequireUserPermission(GuildPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageChannels, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [Summary("Kick the up to 500 users from their talks")]
        [Command("boottheherd", RunMode = RunMode.Async)]
        [Alias("boot", "end")]
        public async Task EndSessionTalkCommand()
        {
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
            //Strip their roles
            foreach (var user in users)
            {
                IEnumerable<IRole> iRoles = Context.Guild.Roles.Where(e => e.Name == Program.RoleIDs.FirstOrDefault(f => e.Name.Contains(f,StringComparison.InvariantCultureIgnoreCase)));
                await user.RemoveRolesAsync(iRoles);
            }
            IEnumerable<IGuildChannel> iChans = chans.Where(e => (Program.ChannelIDs.Contains(e.Name)));

            foreach (var room in iChans)
            {
                await room.DeleteAsync();
            }

            IGuildChannel cExploitDev = chans.Where(e => e.Name.Contains("ExploitDev",
               StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IGuildChannel cCTFIntro = chans.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IGuildChannel cCyber101 = chans.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (cExploitDev == null)
            {
                //create the channels
                ITextChannel tExploitDev = await Context.Guild.CreateTextChannelAsync("ExploitDev");
                if (tExploitDev != null)
                {
                    var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "ExploitDev");
                    var perms = new OverwritePermissions(
                        sendMessages: PermValue.Allow,
                        addReactions: PermValue.Allow,
                        viewChannel: PermValue.Allow
                    );
                    await tExploitDev.AddPermissionOverwriteAsync(botrole, botperms);
                    await tExploitDev.AddPermissionOverwriteAsync(defaultrole, defaultperms);
                    await tExploitDev.AddPermissionOverwriteAsync(role, perms);
                    //Program.ChannelIDs.Add(tExploitDev);
                }
            }
            if (cCTFIntro == null)
            {
                ITextChannel tCTFIntro = await Context.Guild.CreateTextChannelAsync("CTFIntro");
                if (tCTFIntro != null)
                {
                    var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "CTFIntro");
                    var perms = new OverwritePermissions(
                        sendMessages: PermValue.Allow,
                        addReactions: PermValue.Allow,
                        viewChannel: PermValue.Allow
                    );
                    await tCTFIntro.AddPermissionOverwriteAsync(botrole, botperms);
                    await tCTFIntro.AddPermissionOverwriteAsync(defaultrole, defaultperms);
                    await tCTFIntro.AddPermissionOverwriteAsync(role, perms);
                    //Program.ChannelIDs.Add(tCTFIntro);
                }
            }
            if (cCyber101 == null)
            {
                ITextChannel tCyber101 = await Context.Guild.CreateTextChannelAsync("Cyber101");
                if (tCyber101 != null)
                {
                    var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Cyber101");
                    var perms = new OverwritePermissions(
                        sendMessages: PermValue.Allow,
                        addReactions: PermValue.Allow,
                        viewChannel: PermValue.Allow
                    );
                    await tCyber101.AddPermissionOverwriteAsync(botrole, botperms);
                    await tCyber101.AddPermissionOverwriteAsync(defaultrole, defaultperms);
                    await tCyber101.AddPermissionOverwriteAsync(role, perms);
                    //Program.ChannelIDs.Add(tCyber101);
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
            IGuildChannel cExploitDev = chans.Where(e => e.Name.Contains("ExploitDev",
               StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IGuildChannel cCTFIntro = chans.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IGuildChannel cCyber101 = chans.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (cExploitDev == null)
            {
                //create the channels
                ITextChannel tExploitDev = await Context.Guild.CreateTextChannelAsync("ExploitDev");
                if (tExploitDev != null)
                {
                    //create the roles
                    IRole rTryExploitDev = Context.Guild.Roles.Where(e => e.Name.Contains("ExploitDev",
                        StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (rTryExploitDev == null)
                    {
                        IRole rExploitDev = await Context.Guild.CreateRoleAsync(name: "ExploitDev", color: Color.Teal, isMentionable: false);
                        
                    }
                    var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "ExploitDev");
                    var perms = new OverwritePermissions(
                        sendMessages: PermValue.Allow,
                        addReactions: PermValue.Allow,
                        viewChannel: PermValue.Allow
                    );
                    await tExploitDev.AddPermissionOverwriteAsync(botrole, botperms);
                    await tExploitDev.AddPermissionOverwriteAsync(defaultrole, defaultperms);
                    await tExploitDev.AddPermissionOverwriteAsync(role, perms);
                    //Program.ChannelIDs.Add(tExploitDev);
                }
            }
            if (cCTFIntro == null)
            {
                ITextChannel tCTFIntro = await Context.Guild.CreateTextChannelAsync("CTFIntro");
                if (tCTFIntro != null)
                {
                    IRole rTryCTFIntro = Context.Guild.Roles.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (rTryCTFIntro == null)
                    {
                        IRole rCTFIntro = await Context.Guild.CreateRoleAsync(name: "CTFIntro", color: Color.Purple, isMentionable: false);
                        
                    }
                    var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "CTFIntro");
                    var perms = new OverwritePermissions(
                        sendMessages: PermValue.Allow,
                        addReactions: PermValue.Allow,
                        viewChannel: PermValue.Allow
                    );
                    await tCTFIntro.AddPermissionOverwriteAsync(botrole, botperms);
                    await tCTFIntro.AddPermissionOverwriteAsync(defaultrole, defaultperms);
                    await tCTFIntro.AddPermissionOverwriteAsync(role, perms);
                    //Program.ChannelIDs.Add(tCTFIntro);
                }
            }
            if (cCyber101 == null)
            {
                ITextChannel tCyber101 = await Context.Guild.CreateTextChannelAsync("Cyber101");
                if (tCyber101 != null)
                {
                    IRole rTryCyber101 = Context.Guild.Roles.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (rTryCyber101 == null)
                    {
                        IRole rCyber101 = await Context.Guild.CreateRoleAsync(name: "Cyber101", color: Color.DarkMagenta, isMentionable: false);
                        
                    }
                    var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Cyber101");
                    var perms = new OverwritePermissions(
                        sendMessages: PermValue.Allow,
                        addReactions: PermValue.Allow,
                        viewChannel: PermValue.Allow
                    );
                    await tCyber101.AddPermissionOverwriteAsync(botrole, botperms);
                    await tCyber101.AddPermissionOverwriteAsync(defaultrole, defaultperms);
                    await tCyber101.AddPermissionOverwriteAsync(role, perms);
                    //Program.ChannelIDs.Add(tCyber101);
                }
            }
            IRole rTryWaitExploitDev = Context.Guild.Roles.Where(e => e.Name.Contains("WaitExploitDev",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitExploitDev == null)
            {
                IRole rExploitDev = await Context.Guild.CreateRoleAsync(name: "WaitExploitDev", color: Color.Teal, isMentionable: false);
                
            }
            IRole rTryWaitCTFIntro = Context.Guild.Roles.Where(e => e.Name.Contains("WaitCTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitCTFIntro == null)
            {
                IRole rCTFIntro = await Context.Guild.CreateRoleAsync(name: "WaitCTFIntro", color: Color.Purple, isMentionable: false);
                
            }
            IRole rTryWaitCyber101 = Context.Guild.Roles.Where(e => e.Name.Contains("WaitCyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitCyber101 == null)
            {
                IRole rCyber101 = await Context.Guild.CreateRoleAsync(name: "WaitCyber101", color: Color.DarkMagenta, isMentionable: false);
                
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
            IRole rTryExploitDev = Context.Guild.Roles.Where(e => e.Name.Contains("ExploitDev",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryExploitDev != null)
            {
                IRole rExploitDev = Context.Guild.GetRole(rTryExploitDev.Id);
                await rExploitDev.DeleteAsync();
            }
            IRole rTryCTFIntro = Context.Guild.Roles.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryCTFIntro != null)
            {
                IRole rCTFIntro = Context.Guild.GetRole(rTryCTFIntro.Id);
                await rCTFIntro.DeleteAsync();
            }
            IRole rTryCyber101 = Context.Guild.Roles.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryCyber101 != null)
            {
                IRole rCyber101 = Context.Guild.GetRole(rTryCyber101.Id);
                await rCyber101.DeleteAsync();
            }

            IRole rTryWaitExploitDev = Context.Guild.Roles.Where(e => e.Name.Contains("WaitExploitDev",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitExploitDev != null)
            {
                IRole rExploitDev = Context.Guild.GetRole(rTryWaitExploitDev.Id);
                await rExploitDev.DeleteAsync();
            }
            IRole rTryWaitCTFIntro = Context.Guild.Roles.Where(e => e.Name.Contains("WaitCTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitCTFIntro != null)
            {
                IRole rCTFIntro = Context.Guild.GetRole(rTryWaitCTFIntro.Id);
                await rCTFIntro.DeleteAsync();
            }
            IRole rTryWaitCyber101 = Context.Guild.Roles.Where(e => e.Name.Contains("WaitCyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitCyber101 != null)
            {
                IRole rCyber101 = Context.Guild.GetRole(rTryWaitCyber101.Id);
                await rCyber101.DeleteAsync();
            }

            var chans = await Context.Guild.GetChannelsAsync();
            IGuildChannel cExploitDev =  chans.Where(e => e.Name.Contains("ExploitDev",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IGuildChannel cCTFIntro = chans.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IGuildChannel cCyber101 = chans.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            
            if (cExploitDev != null)
            {
                await cExploitDev.DeleteAsync();
            }
            if (cCTFIntro != null)
            {
                await cCTFIntro.DeleteAsync();
            }
            if (cCyber101 != null)
            {
                await cCyber101.DeleteAsync();
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
            List<IAuditLogEntry> ctfintro = new List<IAuditLogEntry>(500);
            List<IAuditLogEntry> cyber101 = new List<IAuditLogEntry>(500);
            List<IAuditLogEntry> exploitdev = new List<IAuditLogEntry>(500);

            IRole rTryExploitDev = null;
            IRole rExploitDev = null;
            IRole rTryCTFIntro = null;
            IRole rCTFIntro = null;
            IRole rTryCyber101 = null;
            IRole rCyber101 = null;
            IRole rTryWaitExploitDev = null;
            IRole rWaitExploitDev = null;
            IRole rTryWaitCTFIntro = null;
            IRole rWaitCTFIntro = null;
            IRole rTryWaitCyber101 = null;
            IRole rWaitCyber101 = null;
            //get all waitlisted ctfintro
            //if user has role ...
            //
            //get all waitlisted cyber101
            //get all waitlisted exploitdev

            IEnumerable<IAuditLogEntry> logs = await Context.Guild.GetAuditLogsAsync(limit: 10000);
            foreach(var log in logs)
            {
                if(log.CreatedAt.DateTime < DateTime.Now.Subtract(new TimeSpan(1,0,0,0)))
                {
                    break;
                }
                if(log.Action == ActionType.MemberRoleUpdated)
                {
                    IAuditLogData data = log.Data;
                    Discord.Rest.MemberRoleAuditLogData memberRoleAuditLogData = log.Data as Discord.Rest.MemberRoleAuditLogData;
                    if (memberRoleAuditLogData.Roles.Any(e => e.Name.Contains("WaitCyber101", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        cyber101.Add(log); 
                    }
                    if (memberRoleAuditLogData.Roles.Any(e => e.Name.Contains("WaitCTFIntro", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        ctfintro.Add(log);
                    }
                    if (memberRoleAuditLogData.Roles.Any(e => e.Name.Contains("WaitExploitDev", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        exploitdev.Add(log);
                    }
                }
            }
            ctfintro.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));
            exploitdev.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));
            cyber101.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));
            
            rTryExploitDev = Context.Guild.Roles.Where(e => e.Name.Contains("ExploitDev",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryExploitDev != null)
            {
                rExploitDev = Context.Guild.GetRole(rTryExploitDev.Id);
                
            }
            rTryCTFIntro = Context.Guild.Roles.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryCTFIntro != null)
            {
                rCTFIntro = Context.Guild.GetRole(rTryCTFIntro.Id);
                
            }
            rTryCyber101 = Context.Guild.Roles.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryCyber101 != null)
            {
                rCyber101 = Context.Guild.GetRole(rTryCyber101.Id);
                
            }
            rTryWaitExploitDev = Context.Guild.Roles.Where(e => e.Name.Contains("WaitExploitDev",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitExploitDev != null)
            {
                rWaitExploitDev = Context.Guild.GetRole(rTryWaitExploitDev.Id);
                
            }
            rTryWaitCTFIntro = Context.Guild.Roles.Where(e => e.Name.Contains("WaitCTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitCTFIntro != null)
            {
                rWaitCTFIntro = Context.Guild.GetRole(rTryWaitCTFIntro.Id);
                
            }
            rTryWaitCyber101 = Context.Guild.Roles.Where(e => e.Name.Contains("WaitCyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryWaitCyber101 != null)
            {
                rWaitCyber101 = Context.Guild.GetRole(rTryWaitCyber101.Id);
                
            }
            foreach (var entry in ctfintro)
            {
                Discord.Rest.MemberRoleAuditLogData memberRoleAuditLogData = entry.Data as Discord.Rest.MemberRoleAuditLogData;
                IGuildUser user = await Context.Guild.GetUserAsync(memberRoleAuditLogData.Target.Id);
                if (user.RoleIds.Any(e => e == rWaitCTFIntro.Id))
                {
                    MemberRoleEditInfo mi = memberRoleAuditLogData.Roles.FirstOrDefault();
                    IRole role = Context.Guild.GetRole(mi.RoleId);
                    await user.RemoveRoleAsync(role);
                    await user.AddRoleAsync(rCTFIntro);
                }
            }
            foreach (var entry in cyber101)
            {
                Discord.Rest.MemberRoleAuditLogData memberRoleAuditLogData = entry.Data as Discord.Rest.MemberRoleAuditLogData;
                IGuildUser user = await Context.Guild.GetUserAsync(memberRoleAuditLogData.Target.Id);
                if (user.RoleIds.Any(e => e == rWaitCyber101.Id))
                {
                    MemberRoleEditInfo mi = memberRoleAuditLogData.Roles.FirstOrDefault();
                    IRole role = Context.Guild.GetRole(mi.RoleId);
                    await user.RemoveRoleAsync(role);
                    await user.AddRoleAsync(rCyber101);
                }
            }
            foreach (var entry in exploitdev)
            {
                Discord.Rest.MemberRoleAuditLogData memberRoleAuditLogData = entry.Data as Discord.Rest.MemberRoleAuditLogData;
                IGuildUser user = await Context.Guild.GetUserAsync(memberRoleAuditLogData.Target.Id);
                if (user.RoleIds.Any(e => e == rWaitExploitDev.Id))
                {
                    MemberRoleEditInfo mi = memberRoleAuditLogData.Roles.FirstOrDefault();
                    IRole role = Context.Guild.GetRole(mi.RoleId);
                    await user.RemoveRoleAsync(role);
                    await user.AddRoleAsync(rExploitDev);
                }
            }
        }
    }
}
