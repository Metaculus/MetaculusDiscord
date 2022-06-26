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

public class AlertService : DiscordClientService
{
    private readonly Data.Data _data;
    private readonly IConfiguration _configuration;
    private Timer? _timer;

    public AlertService(DiscordSocketClient client, ILogger<DiscordClientService> logger, Data.Data data,
        IConfiguration configuration) : base(client,
        logger)
    {
        _data = data;
        _configuration = configuration;
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
        // they can run concurrently because they don't interact 
        Task.Run(Alert<UserQuestionAlert>);
        Task.Run(Alert<ChannelQuestionAlert>);
    }

    /// <summary>
    ///     Fetches all the alerts from the database and sends those that get triggered to the appropriate channels.
    /// </summary>
    /// <typeparam name="T">QuestionAlert type that has a table in the database</typeparam>
    private async Task Alert<T>() where T : QuestionAlert
    {
        // load all personal  alerts to memory 
        var allUserAlerts = await _data.GetAllAlertsAsync<T>();
        // download and parse all questions from the api
        var alertsAndQuestions = JoinWithQuestions(allUserAlerts.ToList());
        // filter for those that have resolved -> send message -> remove from db
        List<Tuple<T, AlertQuestion>> resolved = new();
        List<Tuple<T, AlertQuestion>> ambiguous = new();
        List<Tuple<T, AlertQuestion>> sixHourSwing = new();
        List<Tuple<T, AlertQuestion>> daySwing = new();
        // split 
        var daySwingThreshold = _configuration.GetValue<double>("DaySwingThreshold");
        var sixHourSwingThreshold = _configuration.GetValue<double>("SixHourSwingThreshold");
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
                await _data.TryRemoveAlertAsync(alert);
            }
        });
        await _data.RemoveAlerts(resolved.Select(t => t.Item1));
        await _data.RemoveAlerts(ambiguous.Select(t => t.Item1));
    }

    /// <summary>
    ///     Sends a message either to channel or DM depending on the alert type.
    /// </summary>
    /// <param name="message">Message to be sent</param>
    /// <param name="alert">Alert whose target is sent the message.</param>
    /// <typeparam name="TAlert">QuestionAlert</typeparam>
    private async Task SendAlertMessageAsync<TAlert>(string message, TAlert alert) where TAlert : QuestionAlert
    {
        if (alert is UserQuestionAlert userAlert)
        {
            var target = await Client.GetUserAsync(userAlert.UserId);
            await target.SendMessageAsync(message);
        }
        else if (alert is ChannelQuestionAlert channelAlert)
        {
            if (await Client.GetChannelAsync(channelAlert.ChannelId) is ITextChannel target)
                await target.SendMessageAsync(message);
        }
    }

    public enum AlertKind
    {
        Resolved,
        Ambiguous,
        SixHourSwing,
        DaySwing
    }

    /// <summary>
    ///     Create message for an alert depending on its kind and send it.
    /// </summary>
    private async Task CreateAlertMessageAndSendAsync<T>(Tuple<T, AlertQuestion> t, AlertKind kind)
        where T : QuestionAlert
    {
        var (alert, question) = t;
        string message;
        switch (kind)
        {
            case AlertKind.Resolved:
                message =
                    $"Question {question.Id}: {question.Title} has been resolved.\n The answer is **{question.ValueString()}** \n" +
                    question.ShortUrl();
                break;
            case AlertKind.Ambiguous:
                message = $"Question {question.Id}: {question.Title} has been resolved as **ambiguous** \n" +
                          question.ShortUrl();
                break;
            case AlertKind.SixHourSwing:
                message =
                    $"⚠️Question {question.Id}: {question.Title} has shifted significantly in the past 6 hours! \n" +
                    $"{question.SixHoursOldValueString()} {EmotesUtils.SignEmote(question.SixHourSwing())}  **{question.ValueString()}**\n" +
                    question.ShortUrl();
                break;
            case AlertKind.DaySwing:
                message = $"⚠️Question {question.Id}: {question.Title} has shifted significantly in the past day! \n" +
                          $"{question.DayOldValueString()} {EmotesUtils.SignEmote(question.DaySwing())}  **{question.ValueString()}**\n" +
                          question.ShortUrl();
                break;
            default:
                message = "";
                break;
        }

        await SendAlertMessageAsync(message, alert);
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
        var allQuestionIds = alerts.AsParallel().Select(x => x.QuestionId).Distinct();
        var questions = allQuestionIds.Select(ApiUtils.GetAlertQuestionFromIdAsync).Where(y => y.Result != null)
            .ToArray();
        Task.WaitAll(questions);
        var filtered = questions.Where(y => y.Result != null);
        // inner join
        return alerts.Join(filtered, x => x.QuestionId, y => y.Result!.Id,
            (x, y) => new Tuple<T, AlertQuestion>(x, y.Result!));
    }
}