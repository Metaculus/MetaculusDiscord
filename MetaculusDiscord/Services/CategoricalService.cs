using System.Timers;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace MetaculusDiscord.Services;

public class CategoricalService : DiscordClientService
{
    private IConfiguration _configuration;
    private Data.Data _data;
    private HttpClient _httpClient = new();

    public CategoricalService(DiscordSocketClient client, ILogger<CategoricalService> logger, Data.Data data,
        IConfiguration configuration) : base(client,
        logger)
    {
        _data = data;
        _configuration = configuration;
    }

#pragma warning disable CS1998
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
#pragma warning restore CS1998
    {
        #if DEBUG
        // Check once per day and  
        // 30 seconds 
        var timer = new Timer(30 * 1000);
        #else
        var timer = new System.Timers.Timer(24 * 60 * 60 * 1000);
#endif
        timer.Elapsed += Digest;
        timer.Start();
    }

    private void Digest(object? sender, ElapsedEventArgs e)
    {
        throw new NotImplementedException();
    }
}