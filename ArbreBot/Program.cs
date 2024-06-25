/*
https://www.c-sharpcorner.com/article/how-to-write-your-own-discord-bot-on-net/
dotnet build
dotnet run
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows.Input;

namespace ArbreGenia;

class Program
{
    public static List<string> ChannelName = new List<string>() { "arbre-généalogique", "test-prive", "cmd_bot" };
        //"bot-debug";
    public static HashSet<string> DiscordUIDBannedTmp = new();
    public static HashSet<string> DiscordUIDAdmin = new();

    public Graphe G = new Graphe();
    public string G_backup = "";
    public string G_path = "";

    private DiscordSocketClient? _client;
    private CommandService? _commands;
    private IServiceProvider? _services;

    static async Task Main(string[] args)
    {
        var program = new Program();
        await program.RunBotAsync();
    }

    void Load()
    {
        // Todo : load G commands
        G = new Graphe();
        G_path = Path.Combine(Directory.GetCurrentDirectory(), "Save", "lvdiweeb.txt");
        G_backup = Path.Combine(Directory.GetCurrentDirectory(), "Backup");

        Console.WriteLine("G_path : " + G_path);
        Console.WriteLine("G_backup : " + G_backup);
        StringBuilder sb = new StringBuilder();

        DiscordUIDBannedTmp = new HashSet<string>();
        DiscordUIDAdmin = new HashSet<string>() { "mewily#0000" };

        int old_count = G.Length;
        if(G.Load(sb, G_path) == false) 
        {
            Console.WriteLine(sb.ToString());
            throw new Exception("Error in command");
        }
        Console.WriteLine((G.Length - old_count) + " personnes chargées");
    }

    public async Task RunBotAsync()
    {
        Load();
        
        var config = new DiscordSocketConfig()
        {
            // Other config options can be presented here.
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
            //GatewayIntents = GatewayIntents.AllUnprivileged |  GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _commands = new CommandService();

        string token = "TOKEN DU BOT";  throw new Exception("Remplacer cette ligne par la chaine de texte du token du bot");

        _client.Log += LogAsync;

        await RegisterCommandsAsync();

        await _client.LoginAsync(TokenType.Bot, token);

        await _client.StartAsync();

        await Task.Delay(-1);
    }
    public async Task RegisterCommandsAsync()
    {
        _client!.MessageReceived += MessageReceivedAsync;
        await _commands!.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        Command? c = null;

        try
        {
            // The bot should never respond to itself.
            if (message.Author.Id == _client!.CurrentUser.Id || message.Author.IsBot)
            {
                return;
            }

            if (ChannelName.Contains(message.Channel.Name.ToLower()) == false)
            {
                //Console.WriteLine(ChannelName + " is the wrong channel");
                return;
            }

            StringBuilder str = new StringBuilder();
            if (message.Content.StartsWith(Command.Prefix) == false)
            {
                return;
                //str.AppendLine("Les commandes doivent commencer par: `" + Command.Prefix + "`");
            }

            string uid_discord = message.Author.Username + "#" + message.Author.Discriminator.ToString().PadLeft(4, '0');
            if (DiscordUIDBannedTmp.Contains(uid_discord) == false)
            {
                try
                {
                    c = new Command(str, message.Content.Substring(Command.Prefix.Length));
                    if (G.Execute(c, DiscordUIDAdmin.Contains(uid_discord)) == false)
                    {
                        str.AppendLine("Essaie `" + Command.Prefix + "help` pour avoir de l'aide");
                    }
                    else
                    {
                        if (c.HaveSideEffect || c.IsUndo) 
                        {
                            string backup_path = Path.Combine(G_backup, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt");
                            try
                            {
                                G.Save(backup_path);
                            }
                            catch
                            {
                                Console.WriteLine("Failed to write backup at " + backup_path);
                            }
                            G.Save(G_path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    str.AppendLine(ex.Message);
                    str.AppendLine("in");
                    str.AppendLine(ex.Source);
                    str.AppendLine("Bip Bop, erreur");
                }
            }
            else 
            {
                str.AppendLine("Tu es ban !");
            }

            var msg = str.ToString();
            if(msg.Length > 0) 
            {
                await message.Channel.SendMessageAsync(str.ToString());
            }

            if (c != null && c.ShouldSendSaveFile)
            {
                await message.Channel.SendFileAsync(G_path);
            }
        }
        catch (Exception e)
        {
            try
            {
                await message.Channel.SendMessageAsync("Bip Bop, erreur : \n" + e.Message);
            }
            catch (Exception ex)
            {

            }
        }

        if (c.ShouldStop) 
        {
            _client!.Dispose();
            _client = null;
            _commands = null;
            throw new Exception("bye bye (stop commande)");
        }
    }
}