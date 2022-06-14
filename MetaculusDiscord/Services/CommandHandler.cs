using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using MetaculusDiscord.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MetaculusDiscord.Services;

public class CommandHandler : DiscordClientService
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger<DiscordClientService> _logger;
    private readonly IServiceProvider _provider;
    private readonly CommandService _service;
    private readonly IConfiguration _configuration;
    private readonly Data.Data _data;

    public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration configuration, ILogger<DiscordClientService> logger,Data.Data data)
        : base(client, logger)
    {
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
        await _service.AddModuleAsync(typeof(SearchCommands), _provider); // adds modules using reflection
        // await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider); // adds modules using reflection
    }

   
    private async Task OnReact(Cacheable<IUserMessage, ulong> messageC, Cacheable<IMessageChannel, ulong> channelC, SocketReaction reaction)
    {
        // work only with cached messages (we cache N last messages)
        if (!(messageC.HasValue)) return;
        if (!(channelC.HasValue)) return;
        var message = messageC.Value;
        var channel = channelC.Value;
        // user is not metaculus and it is on a metaculus message 
        if (_client.CurrentUser.Id != reaction.UserId && message.Author.Id == _client.CurrentUser.Id)
        {
            // await channel.SendMessageAsync(reaction.Emote.Name);
            int selected= -1;
            foreach (var (i,e) in Utils.EmotesUtils.GetEmojiNumbersDict())
            {
                if (e.Name.Equals(reaction.Emote.Name)) selected = i;
            }

            if (selected == -1) return;
            
            await channel.SendMessageAsync(_data.GetResponse(message.Id).Links[selected-1]); // this should actually pick the one selected  
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