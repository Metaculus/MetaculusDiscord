using System.Timers;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using MetaculusDiscord.Model;
using Microsoft.Extensions.Logging;

namespace MetaculusDiscord.Services;

public class AlertService : DiscordClientService
{
    private Data.Data _data;

    public AlertService(DiscordSocketClient client, ILogger<DiscordClientService> logger, Data.Data data) : base(client,
        logger)
    {
        _data = data;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var aTimer = // 6 hours
            new System.Timers.Timer(6 * 60 * 60 * 1000);
        aTimer.Elapsed += ProcessUpdates;
        aTimer.Start();
    }

    private void ProcessUpdates(object? sender, ElapsedEventArgs e)
    {
        PersonalAlerts();
        // ChannelAlerts();
    }

    private void PersonalAlerts()
    {
        // load all personal  alerts to memory 
        // filter for those that have resolved -> send message -> remove from db
        // filter for those that have updated significantly -> send message -> update db
        // var alerts = _data.LoadMetaculusPersonalAlerts();
        // var questions = LoadMetaculusQuestions(alerts.Select(a => a.QuestionId).Distinct().ToList());
        // var alertQuestionPairs= alerts.Join(questions, a => a.QuestionId, q => q.Id, (a, q) => new {a, q});
        // foreach ((PersonalQuestionAlert a, Question q) in alertQuestionPairs)
        // {
        //     if (a.Resolved)
        //     {
        //         // send message
        //         // remove from db
        //     }
        //     else if (a.Updated)
        //     {
        //         // send message
        //         // update db
        //     }
        // }
    }
}