using System.Dynamic;
using System.Reflection;
using System.Threading.Channels;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using MetaculusDiscord.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IResult = Discord.Commands.IResult;

namespace MetaculusDiscord.Services;

public class CommandHandler : DiscordClientService
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger<DiscordClientService> _logger;
    private readonly IServiceProvider _provider;
    private readonly CommandService _service;
    private readonly InteractionService _interactionService;
    private readonly IConfiguration _configuration;
    private readonly Data.Data _data;

    public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,InteractionService interactionService, IConfiguration configuration, ILogger<DiscordClientService> logger,Data.Data data)
        : base(client, logger)
    {
        _interactionService = interactionService;
        _provider = provider;
        _client = client;
        _service = service;
        _configuration = configuration;
        _logger = logger;
        _data = data;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.ReactionAdded += OnReact;
        _client.MessageReceived += OnMessage;
        // _client.Ready += MakeCommand;
        // _client.SlashCommandExecuted += SlashCommandExecuted;
        // _interactionService.SlashCommandExecuted += SlashCommandExecuted;
        await _service.AddModuleAsync(typeof(SearchCommands), _provider); 
        await _service.AddModuleAsync(typeof(UtilCommands), _provider); 
        await _service.AddModuleAsync(typeof(AlertCommands), _provider);
        // await _service.AddModuleAsync(typeof(SlashModule), _provider);

    }

    // private async Task SlashCommandExecuted(SlashCommandInfo slashCommandInfo, IInteractionContext interactionContext, Discord.Interactions.IResult result) => await Task.Run(()=>
       // Console.WriteLine("somethin"));
    // private async Task OnSlashCommandExecuted(SocketSlashCommand command)
    // {
    //     Console.WriteLine("hello this is slash command");
    //     if (command.CommandName == "metaculus")
    //     {
    //         await command.RespondAsync("you executed a metaculus search!");
    //     }
    // }

    private async Task MakeCommand()
    {
        // Let's do our global command
        var globalCommand = new SlashCommandBuilder();
        globalCommand.WithName("metaculus");
        globalCommand.WithDescription("Search metaculus!");
        try
        {
            // _client
            await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
            // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            // TODO: change for production
        }
        catch(HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }
    }

    // 1. je to validní emote, 2. je to od validního uživatele 3. checknout databázi, jestli daný message je zajímavý. Pokud je vše splněno lze vykonat akci emotu s pomocí messaage.
    private async Task OnReact(Cacheable<IUserMessage, ulong> messageC, Cacheable<IMessageChannel, ulong> channelC, SocketReaction reaction)
    {
        // note: reaction emote name is not discord emote name!
        var message = await messageC.GetOrDownloadAsync();
        // user is not metaculus and it is on a metaculus message 
        if (_client.CurrentUser.Id != reaction.UserId && message.Author.Id == _client.CurrentUser.Id)
        {
            
            
            // now check if the message is in storage compatible with the action 
            // await channel.SendMessageAsync(reaction.Emote.Name);
            int selected= -1;
            foreach (var (i,e) in Utils.EmotesUtils.GetEmojiNumbersDict())
            {
                if (e.Name.Equals(reaction.Emote.Name)) selected = i;
            }
            if (selected == -1) return;
            var channel = await channelC.GetOrDownloadAsync();
            await channel.SendMessageAsync(_data.GetResponse(message.Id).Links[selected-1]); 
        }
        

    }

    private async Task OnMessage(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage socketUserMessage) return; 
        if (socketUserMessage.Source != MessageSource.User) return; // handle only messages from users
        int argPos = 0;
        _logger.Log(LogLevel.Debug,message:"Message registered");
        if (!socketUserMessage.HasStringPrefix(_configuration["Prefix"],ref argPos) ) return;
        _logger.Log(LogLevel.Debug,message:"Message with Prefix registered");

        var context = new SocketCommandContext(_client,socketUserMessage);
        await _service.ExecuteAsync(context, argPos, _provider);
    }
}