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

    public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration configuration, ILogger<DiscordClientService> logger)
        : base(client, logger)
    {
        _provider = provider;
        _client = client;
        _service = service;
        _configuration = configuration;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.ReactionAdded += OnReact;
        _client.MessageReceived += OnMessage;
        await _service.AddModuleAsync(typeof(RandomCommandsModule), _provider); // adds modules using reflection
        // await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider); // adds modules using reflection
    }

    private Task OnReact(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        throw new NotImplementedException();
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