using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WatchPartyBot.Services;

namespace WatchPartyBot
{
    public class RoleIdName : IEquatable<RoleIdName>
    {
        public ulong Id;
        public string Role;
        public RoleIdName(ulong id, string role)
        {
            Id = id;
            Role = role;
        }

        public bool Equals([AllowNull] RoleIdName obj)
        {
            return obj is RoleIdName && this.Role.Contains(obj.Role,StringComparison.InvariantCultureIgnoreCase);
        }
        
    }
    class Program
    {
        // setup our fields we assign later
        private readonly IConfiguration _config;
        private DiscordSocketClient _client;
        
        //RoleId List
        public static HashSet<RoleIdName> RoleIDs = new HashSet<RoleIdName>();
        public static HashSet<RoleIdName> ChannelIDs = new HashSet<RoleIdName>();
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            _config = _builder.Build();
        }

        public async Task MainAsync()
        {
            // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
            using (var services = ConfigureServices())
            {
                // get the client and assign to client 
                // you get the services via GetRequiredService<T>
                var client = services.GetRequiredService<DiscordSocketClient>();
                _client = client;
                
                // setup logging and the ready event
                client.Log += LogAsync;
                client.Ready += ReadyAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                // this is where we get the Token value from the configuration file, and start the bot
                await client.LoginAsync(TokenType.Bot, _config["Token"]);
                await client.StartAsync();

                // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
                await services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(-1);
            }
        }
        private void GetChannelsPopulateList()
        {
            var guilds = _client.Guilds.Where(e => e.Name.Contains("vesh"));
            var guild = guilds.FirstOrDefault();
            var roles = guild.Roles;
            var chans = guild.Channels;
            IGuildChannel cExploitDev = chans.Where(e => e.Name.Contains("ExploitDev",
               StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IGuildChannel cCTFIntro = chans.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IGuildChannel cCyber101 = chans.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IRole rExploitDev = roles.Where(e => e.Name.Contains("ExploitDev",
               StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IRole rCTFIntro = roles.Where(e => e.Name.Contains("CTFIntro",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            IRole rCyber101 = roles.Where(e => e.Name.Contains("Cyber101",
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (cExploitDev != null)
            {
                Program.ChannelIDs.Add(new RoleIdName(cExploitDev.Id, cExploitDev.Name));
            }
            if (cCTFIntro != null)
            {
                Program.ChannelIDs.Add(new RoleIdName(cCTFIntro.Id, cCTFIntro.Name));
            }
            if (cCyber101 != null)
            {
                Program.ChannelIDs.Add(new RoleIdName(cCyber101.Id, cCyber101.Name));
            }

            //create the roles
            if (rExploitDev != null)
            {
                Program.RoleIDs.Add(new RoleIdName(rExploitDev.Id, rExploitDev.Name));
            }
            if (rCTFIntro != null)
            {
                Program.RoleIDs.Add(new RoleIdName(rCTFIntro.Id, rCTFIntro.Name));
            }
            if (rCyber101 != null)
            {
                Program.RoleIDs.Add(new RoleIdName(rCyber101.Id, rCyber101.Name));
            }
        }
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
        
        private Task ReadyAsync()
        {
            Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
            GetChannelsPopulateList();            
            return Task.CompletedTask;
        }

        // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
        private ServiceProvider ConfigureServices()
        {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using csharpi.Services;
            // the config we build is also added, which comes in handy for setting the command prefix!
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }
    }

}
