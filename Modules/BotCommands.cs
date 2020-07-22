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
            foreach(ulong id in user.RoleIds)
            {
                if(Program.RoleIDs.Any(e => (e.Id == id)))
                {
                    IRole rId = Context.Guild.GetRole(id);
                    await user.RemoveRoleAsync(rId);
                }
            }
            //check if role requested is one of the ones allowed
            if (Program.RoleIDs.Any(e => e.Role.Contains(userRoom,StringComparison.InvariantCultureIgnoreCase)))
            {
                IRole rId = Context.Guild.GetRole(Program.RoleIDs.Where(f => f.Role.Contains(userRoom, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Id);
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
        [Summary("Set up initial roles and rooms wait in [ExploitDev, CTFIntro, Cyber101]")]
        [Command("setuproom", RunMode = RunMode.Async)]
        [Alias("initialize", "setup")]
        public async Task SetupRolesRoomsCommand()
        {
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
                    Program.ChannelIDs.Add(new RoleIdName(tExploitDev.Id, tExploitDev.Name));
                }
            }
            if (cCTFIntro == null)
            {
                ITextChannel tCTFIntro = await Context.Guild.CreateTextChannelAsync("CTFIntro");
                if (tCTFIntro != null)
                {
                    Program.ChannelIDs.Add(new RoleIdName(tCTFIntro.Id, tCTFIntro.Name));
                }
            }
            if (cCyber101 == null)
            {
                ITextChannel tCyber101 = await Context.Guild.CreateTextChannelAsync("Cyber101");
                if (tCyber101 != null)
                {
                    Program.ChannelIDs.Add(new RoleIdName(tCyber101.Id, tCyber101.Name));
                }
            }

            //create the roles
            IRole rTryExploitDev = Context.Guild.Roles.Where(e => e.Name.Contains("ExploitDev",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryExploitDev == null)
            {
                IRole rExploitDev = await Context.Guild.CreateRoleAsync(name: "ExploitDev", color: Color.Teal, isMentionable: false);
                if (rExploitDev != null)
                {
                    Program.RoleIDs.Add(new RoleIdName(rExploitDev.Id, rExploitDev.Name));
                }
            }
            IRole rTryCTFIntro = Context.Guild.Roles.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryCTFIntro == null)
            {
                IRole rCTFIntro = await Context.Guild.CreateRoleAsync(name: "CTFIntro", color: Color.Purple, isMentionable: false);
                if (rCTFIntro != null)
                {
                    Program.RoleIDs.Add(new RoleIdName(rCTFIntro.Id, rCTFIntro.Name));
                }
            }
            IRole rTryCyber101 = Context.Guild.Roles.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rTryCyber101 == null)
            {
                IRole rCyber101 = await Context.Guild.CreateRoleAsync(name: "Cyber101", color: Color.DarkMagenta, isMentionable: false);
                if (rCyber101 != null)
                {
                    Program.RoleIDs.Add(new RoleIdName(rCyber101.Id, rCyber101.Name));
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
            Program.RoleIDs = new HashSet<RoleIdName>();
            Program.ChannelIDs = new HashSet<RoleIdName>();
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
                users.Add(user);
            }
        }
    }
}
