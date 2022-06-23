using System.Timers;
using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using MetaculusDiscord.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MetaculusDiscord.Services;

public class AlertService : DiscordClientService
{
    private Data.Data _data;
    private HttpClient _httpClient = new();

    public AlertService(DiscordSocketClient client, ILogger<DiscordClientService> logger, Data.Data data) : base(client,
        logger)
    {
        _data = data;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 6 hours
        // var timer = new System.Timers.Timer(6 * 60 * 60 * 1000);
        // 30 seconds 
        var timer = new System.Timers.Timer(30 * 1000);
        timer.Elapsed += ProcessUpdates;
        timer.Start();
    }

    private void ProcessUpdates(object? sender, ElapsedEventArgs e)
    {
        UserAlerts();
        //todo  ChannelAlerts();
    }

    // todo consider conditional compilation for debugging
    // todo refactor for DRY and reusability for all alerts
    private async void UserAlerts()
    {
        // load all personal  alerts to memory 
        var allUserAlerts = await _data.GetAllUserQuestionAlertsAsync();
        // download and parse all questions from the api
        var userAlertsWithQuestions = JoinWithQuestions(allUserAlerts);
        // filter for those that have resolved -> send message -> remove from db
        List<Tuple<UserQuestionAlert, AlertQuestion>> resolved = new();
        List<Tuple<UserQuestionAlert, AlertQuestion>> ambiguous = new();
        List<Tuple<UserQuestionAlert, AlertQuestion>> smallSwing = new();
        List<Tuple<UserQuestionAlert, AlertQuestion>> bigSwing = new();
        // split 
        foreach (var item in userAlertsWithQuestions)
        {
            var (alert, question) = item;
            if (question.Resolved is null)
            {
                // send that it's ambiguous 
                ambiguous.Add(item);
            }
            else if (question.Resolved.Value)
            {
                resolved.Add(item);
            }
            else
            {
                if (question.Is6HourSwing())
                    smallSwing.Add(item);
                else if (question.IsDaySwing()) bigSwing.Add(item);
            }
        }

        // send messages
        resolved.ForEach(async item => await SendResolvedMessageAsync(item));
        // Parallel.ForEach(resolved, SendResolvedMessageAsync);
        // Parallel.ForEach(ambiguous, SendAmbiguousMessageAsync);
        // Parallel.ForEach(smallSwing, SendSmallSwingMessageAsync);
        // Parallel.ForEach(bigSwing, SendBigSwingMessageAsync);

        // update db //todo DRY!!!
        Parallel.ForEach(resolved, async (tup, _) =>
        {
            var (alert, question) = tup;
            await _data.TryRemoveUserQuestionAlertAsync(alert);
        });
        Parallel.ForEach(ambiguous, async (tup, _) =>
        {
            var (alert, question) = tup;
            await _data.TryRemoveUserQuestionAlertAsync(alert);
        });
    }

    private async Task SendResolvedMessageAsync(Tuple<UserQuestionAlert, AlertQuestion> tup)
    {
        var (alert, question) = tup;
        var target = await Client.GetUserAsync(alert.UserId);
        var message = $"{question.Title} has been resolved. The answer is {question}"; //todo make text
        await target.SendMessageAsync(message);
    }

    private async Task SendAmbiguousMessageAsync(Tuple<UserQuestionAlert, AlertQuestion> tup)
    {
        var (alert, question) = tup;
        var target = await Client.GetUserAsync(alert.UserId);
        var message = $"{question.Title} has been resolved. The answer is {question}"; //todo make text
        await target.SendMessageAsync(message);
    }

    private async Task SendSmallSwingMessageAsync(Tuple<UserQuestionAlert, AlertQuestion> tup)
    {
        var (alert, question) = tup;
        var target = await Client.GetUserAsync(alert.UserId);
        var message = $"{question.Title} has been resolved. The answer is {question}"; //todo make text
        await target.SendMessageAsync(message);
    }

    private async Task SendBigSwingMessageAsync(Tuple<UserQuestionAlert, AlertQuestion> tup)
    {
        var (alert, question) = tup;
        var target = await Client.GetUserAsync(alert.UserId);
        var message = $"{question.Title} has been resolved. The answer is {question}"; //todo make text
        await target.SendMessageAsync(message);
        throw new NotImplementedException();
    }

// todo this is sus and not DRY
    /// <summary>
    /// Downloads questions in parallel from the api and joins them with the alerts.
    /// </summary>
    /// <param name="userAlerts">User Alerts</param>
    /// <returns>IEnumerable of pairs of Alerts and Questions </returns>
    private IEnumerable<Tuple<UserQuestionAlert, AlertQuestion>> JoinWithQuestions(
        IEnumerable<UserQuestionAlert> userAlerts)
    {
        var userQuestionAlerts = userAlerts.ToList();
        var allQuestionIds = userQuestionAlerts.AsParallel().Select(x => x.QuestionId).Distinct();
        var questions = allQuestionIds.Select(GetQuestionFromIdAsync).Where(y => y.Result != null).ToArray();
        Task.WaitAll(questions);
        var filtered = questions.Where(y => y.Result != null);
        return userQuestionAlerts.Join(filtered, x => x.QuestionId, y => y.Result!.Id,
            (x, y) => new Tuple<UserQuestionAlert, AlertQuestion>(x, y.Result!));
    }
    // todo where does this belong?
    private static async Task<AlertQuestion?> GetQuestionFromIdAsync(long id)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync($"https://www.metaculus.com/api2/questions/{id}");
            // deserialize to dynamic
            var dynamicQuestion = JsonConvert.DeserializeObject<dynamic?>(response);
            return new AlertQuestion(dynamicQuestion);
        }
        catch (Exception e)
        {
            return null;
        }
    }
}