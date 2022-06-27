using System.Timers;
using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using MetaculusDiscord.Model;
using MetaculusDiscord.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace MetaculusDiscord.Services;

public class QuestionAlertService : AlertDiscordClientService 
{
    private Timer? _timer;

    public QuestionAlertService(DiscordSocketClient client, ILogger<QuestionAlertService> logger, Data.Data data,
        IConfiguration configuration) : base(client,
        logger,data,configuration)
    {
    }

    /// <summary>
    ///     Initializes the timer.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
# if DEBUG
        // 30 seconds 
        _timer = new Timer(30 * 1000);
# else
        // 6 hours
        var timer = new System.Timers.Timer(6 * 60 * 60 * 1000);
#endif
        _timer.Elapsed += AlertAll;
        _timer.Enabled = true;
        _timer.AutoReset = true;
        _timer.Start();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called every time the timer elapses. Sends alerts to all users and channels.
    /// </summary>
    private void AlertAll(object? sender, ElapsedEventArgs e)
    {
        Logger.LogInformation("Alerting all users and channels");
        // they can run concurrently because they don't interact 
        var t1 =Task.Run(Alert<UserQuestionAlert>);
        var t2 =Task.Run(Alert<ChannelQuestionAlert>);
        try
        {
            Task.WaitAll(t1, t2);
        }
        catch (Exception ex)
        {
            Logger.LogError("Error in alerting users and channels");
            Logger.LogError(ex.ToString());
        }
        Logger.LogInformation("Alerting all users and channels complete");
    }

    /// <summary>
    ///     Fetches all the alerts from the database and sends those that get triggered to the appropriate channels.
    /// </summary>
    /// <typeparam name="T">QuestionAlert type that has a table in the database</typeparam>
    private async Task Alert<T>() where T : QuestionAlert
    {
        // load all personal  alerts to memory 
        var allUserAlerts = await Data.GetAllAlertsAsync<T>();
        // download and parse all questions from the api
        var alertsAndQuestions = JoinWithQuestions(allUserAlerts.ToList());
        // filter for those that have resolved -> send message -> remove from db
        List<Tuple<T, AlertQuestion>> resolved = new();
        List<Tuple<T, AlertQuestion>> ambiguous = new();
        List<Tuple<T, AlertQuestion>> sixHourSwing = new();
        List<Tuple<T, AlertQuestion>> daySwing = new();
        // split 
        var daySwingThreshold = Configuration.GetValue<double>("DaySwingThreshold");
        var sixHourSwingThreshold = Configuration.GetValue<double>("SixHourSwingThreshold");
        foreach (var alertAndQuestion in alertsAndQuestions)
        {
            var question = alertAndQuestion.Item2;
            if (question.Resolved is null)
            {
                ambiguous.Add(alertAndQuestion);
            }
            else if (question.Resolved.Value)
            {
                resolved.Add(alertAndQuestion);
            }
            else
            {
                if (question.Is6HourSwing(sixHourSwingThreshold))
                    sixHourSwing.Add(alertAndQuestion);
                else if (question.IsDaySwing(daySwingThreshold))
                    daySwing.Add(alertAndQuestion);
            }
        }

        // send messages
        resolved.ForEach(t => Task.Run(() => CreateAlertMessageAndSendAsync(t,AlertKind.Resolved)));
        ambiguous.ForEach(t => Task.Run(() => CreateAlertMessageAndSendAsync(t,AlertKind.Ambiguous)));
        sixHourSwing.ForEach(t => Task.Run(() => CreateAlertMessageAndSendAsync(t,AlertKind.SixHourSwing)));
        daySwing.ForEach(t => Task.Run(() => CreateAlertMessageAndSendAsync(t,AlertKind.DaySwing)));


        // update db
        var resolveDeletion = Task.Run(async () =>
        {
            foreach (var item in resolved)
            {
                var (alert, _) = item;
                await Data.TryRemoveAlertAsync(alert);
            }
        });
        await Data.RemoveAlerts(resolved.Select(t => t.Item1));
        await Data.RemoveAlerts(ambiguous.Select(t => t.Item1));
    }



    /// <summary>
    ///     Downloads questions in parallel from the api and inner joins them with the alerts.
    /// </summary>
    /// <param name="alerts">User Alerts</param>
    /// <returns>IEnumerable of pairs of Alerts and Questions </returns>
    private IEnumerable<Tuple<T, AlertQuestion>> JoinWithQuestions<T>(
        List<T> alerts) where T : QuestionAlert
    {
        // get the AlertQuestions in any order
        var allQuestionIds = alerts.Select(x => x.QuestionId).Distinct();
        var questions = allQuestionIds.Select(ApiUtils.GetAlertQuestionFromIdAsync).Where(y => y.Result != null)
            .ToArray();
        Task.WaitAll(questions);
        var filtered = questions.Where(y => y.Result != null);
        // inner join
        return alerts.Join(filtered, x => x.QuestionId, y => y.Result!.Id,
            (x, y) => new Tuple<T, AlertQuestion>(x, y.Result!));
    }
}