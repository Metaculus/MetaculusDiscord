using System.Timers;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace MetaculusDiscord.Services;

public class Alerts : DiscordClientService
{
    private Data.Data _data;
    public Alerts(DiscordSocketClient client, ILogger<DiscordClientService> logger,Data.Data data) : base(client, logger)
    {
        _data = data;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var aTimer = new System.Timers.Timer(60 * 60 * 1000); //One second, (use less to add precision, use more to consume less processor time
        int lastHour = DateTime.Now.Hour;
        aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        aTimer.Start();
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        // something like get all users that wanted to have alerts 
        throw new NotImplementedException();
    }
}