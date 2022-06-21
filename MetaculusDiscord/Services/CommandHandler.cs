using System.Collections;
using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;
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

    public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
        InteractionService interactionService, IConfiguration configuration, ILogger<DiscordClientService> logger,
        Data.Data data)
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
        // _client.Ready += MakeCommand;
        await _service.AddModuleAsync(typeof(Search.SearchCommands), _provider);
        await _service.AddModuleAsync(typeof(UtilCommands), _provider);
        await _service.AddModuleAsync(typeof(AlertCommands), _provider);
        await _interactionService.AddModuleAsync(typeof(Search.SearchSlash), _provider);
        _client.ReactionAdded += OnReactAdded;
        _client.ReactionRemoved += OnReactRemoved;
        _client.MessageReceived += OnMessage;
        _client.SlashCommandExecuted += SlashCommandExecuted;
    }

    // todo: removing alerts from removing reactions 
    private Task OnReactRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2,
        SocketReaction arg3)
    {
        throw new NotImplementedException();
    }

    private async Task SlashCommandExecuted(SocketSlashCommand socketSlashCommand)
    {
        var context = new InteractionContext(_client, socketSlashCommand);
        await _interactionService.ExecuteCommandAsync(context, _provider);
    }


    // 1. je to validní emote, 2. je to od validního uživatele 3. checknout databázi, jestli daný message je zajímavý. Pokud je vše splněno lze vykonat akci emotu s pomocí messaage.
    private async Task OnReactAdded(Cacheable<IUserMessage, ulong> messageC, Cacheable<IMessageChannel, ulong> channelC,
        SocketReaction reaction)
    {
        var message = await messageC.GetOrDownloadAsync();
        // if user is not this bot and it is on a this bot's message, return 
        if (!(_client.CurrentUser.Id != reaction.UserId && message.Author.Id == _client.CurrentUser.Id))
            return;
        if (message.Content.StartsWith("Results:"))
            goto here; //todo remove prasárna :WeirdChamp:
        // if the message does not satisfy the following regex: /https:\/\/www\.metaculus\.com\/questions\/[0-9]+\/.*/gm then return 
        // we're only interested in messages that are metaculus links
        //todo optimize
        if (!Regex.IsMatch(message.Content, @"https:\/\/www\.metaculus\.com\/questions\/[0-9]+\/.*"))
            return;
        // extract the id number from the message and return if it's not possible
        ulong number;
        if (!ulong.TryParse(
                Regex.Match(message.Content, @"https:\/\/www\.metaculus\.com\/questions\/([0-9]+)\/.*")?.Groups[1]
                    ?.Value,
                out number))
            return;
        if (reaction.Emote.Name.Equals(_configuration["UserAlertEmoji"]))
            // CreateAlert() //todo implement this using creating artificial commands
            return;
        here:
        // we want to handle only messages that are a singular metaculus link
        // await channel.SendMessageAsync(reaction.Emote.Name);
        var selected = -1;
        foreach (var (i, e) in Utils.EmotesUtils.GetEmojiNumbersDict())
            if (e.Name.Equals(reaction.Emote.Name))
                selected = i;
        if (selected == -1) return;
        var channel = await channelC.GetOrDownloadAsync();
        if (_data.TryGetResponse(message.Id, out var response))
            await channel.SendMessageAsync(response.Links[selected - 1]);
    }

    /// <summary>
    /// The bot listens for messages and when it finds a message with its prefix, it tries to execute the command. 
    /// </summary>
    /// <param name="socketMessage">Message that caused the event.</param>
    private async Task OnMessage(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage socketUserMessage) return;
        if (socketUserMessage.Source != MessageSource.User) return; // handle only messages from users
        var argPos = 0;
        _logger.Log(LogLevel.Debug, "Message registered");
        if (!socketUserMessage.HasStringPrefix(_configuration["Prefix"], ref argPos)) return;
        _logger.Log(LogLevel.Debug, "Message with Prefix registered");

        var context = new SocketCommandContext(_client, socketUserMessage);
        await _service.ExecuteAsync(context, argPos, _provider);
    }


    /// <summary>
    /// Used only once,I already ran it for this bot,
    /// in case something changes add it as a handler for _client.Ready event
    /// </summary>
    private async Task MakeCommand()
    {
        var globalCommand = new SlashCommandBuilder();
        globalCommand.WithName("metaculus");
        globalCommand.WithDescription("Search metaculus!");
        globalCommand.AddOption("query", ApplicationCommandOptionType.String, "What is your query for metaculus?",
            true);
        try
        {
            // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            _logger.LogError(json);
        }
    }
}