using System.Text.RegularExpressions;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using MetaculusDiscord.Model;
using MetaculusDiscord.Modules;
using MetaculusDiscord.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MetaculusDiscord.Services;

public class InteractionHandler : DiscordClientService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly Data.Data _data;
    private readonly InteractionService _interactionService;
    private readonly ILogger<DiscordClientService> _logger;
    private readonly IServiceProvider _provider;
    private readonly CommandService _service;

    
        public InteractionHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
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
        await _service.AddModuleAsync(typeof(FollowCommands), _provider);
        await _interactionService.AddModuleAsync(typeof(Search.SearchSlash), _provider);
        _client.ReactionAdded += OnReactAdded;
        _client.ReactionRemoved += OnReactRemoved;
        _client.MessageReceived += OnMessage;
        _client.SlashCommandExecuted += SlashCommandExecuted;
    }


    private async Task SlashCommandExecuted(SocketSlashCommand socketSlashCommand)
    {
        var context = new InteractionContext(_client, socketSlashCommand);
        await _interactionService.ExecuteCommandAsync(context, _provider);
    }

/// <summary>
/// Performs actions defined by the reaction which are in case of numeric to results: send the link to the corresponding question adn in case of "
/// </summary>
    private async Task OnReactAdded(Cacheable<IUserMessage, ulong> messageC, Cacheable<IMessageChannel, ulong> channelC,
        SocketReaction reaction)
    {
        var message = await messageC.GetOrDownloadAsync();
        // if user is not this bot and it is on a this bot's message, return 
        if (_client.CurrentUser.Id == reaction.UserId || message.Author.Id != _client.CurrentUser.Id) return;

        if (message.Content.StartsWith("Results:"))
        {
            // handle messages that are a singular metaculus link
            var selected = -1;
            foreach (var (i, e) in EmotesUtils.GetEmojiNumbersDict())
                if (e.Name.Equals(reaction.Emote.Name))
                    // restrict posting the same link multiple times
                    if (message.Reactions.ContainsKey(e) && message.Reactions[e].ReactionCount < 3)  
                        selected = i;
            if (selected == -1) return;
            var channel = await channelC.GetOrDownloadAsync();
            if (_data.TryGetResponse(message.Id, out var response))
                await channel.SendMessageAsync(response.Links[selected - 1]);
            return;
        }

        if (!reaction.Emote.Name.Equals(_configuration["UserAlertEmoji"]))
            return;

        // if the message that is reacted to is not a metaculus link, return
        // extract the id number from the message and return if it's not possible
        if (!long.TryParse(
                Regex.Match(message.Content, @"https:\/\/www\.metaculus\.com\/questions\/([0-9]+)\/.*")?.Groups[1]
                    ?.Value,
                out var questionId))
            return;
        await AddUserAlert(reaction.UserId, questionId); 
    }
/// <summary>
/// If UserAlertEmoji is removed from a link then remove the alert for that question.
/// </summary>
    private async Task OnReactRemoved(Cacheable<IUserMessage, ulong> messageC, Cacheable<IMessageChannel, ulong> chanelC,
        SocketReaction reaction)
{
        var message = await  messageC.GetOrDownloadAsync();
        if (!long.TryParse(
                Regex.Match(message.Content, @"https:\/\/www\.metaculus\.com\/questions\/([0-9]+)\/.*")?.Groups[1]
                    ?.Value,
                out var questionId))
            return;
        await RemoveUserAlert(reaction.UserId, questionId); 
        
    }


// todo refactor to unify with command version 
    private async Task AddUserAlert(ulong reactionUserId, long questionId) 
    {
        var alert = new UserQuestionAlert(){UserId = reactionUserId, QuestionId = questionId};
        var user = await _client.GetUserAsync(reactionUserId);
        if (await _data.TryAddAlertAsync(alert))
        {
            await user.SendMessageAsync($"Alert for question {questionId} set");
        }
        else
        {
            await user.SendMessageAsync($"Error: Alert for question {questionId} already set");
        }
    }
private async Task RemoveUserAlert(ulong reactionUserId, long questionId)
{
    var alert = new UserQuestionAlert(){UserId = reactionUserId, QuestionId = questionId};
    if (await _data.TryRemoveAlertAsync(alert))
    {
        var user = await _client.GetUserAsync(reactionUserId);
        await user.SendMessageAsync($"Alert for question {questionId} removed");
    }
}

    /// <summary>
    ///     The bot listens for messages and when it finds a message with its prefix, it tries to execute the command.
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
    ///     Used only once, I already ran it for this bot,
    ///     in case something changes add it as a handler for _client.Ready event
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