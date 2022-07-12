using System.Diagnostics;
using System.Timers;
using Discord.WebSocket;
using MetaculusDiscord.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace MetaculusDiscord.Services;

/// <summary>
/// Service that every 24 hours checks categories and sends updates.
/// </summary>
public class CategoricalService : AlertService
{
    public CategoricalService(DiscordSocketClient client, ILogger<CategoricalService> logger, Data.Data data,
        IConfiguration configuration) : base(client,
        logger, data, configuration)
    {
    }

    private Timer? _timer;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
#if DEBUG
        // 30 seconds 
        _timer = new Timer(30 * 1000);
#else
        // Check once per day 
        _timer = new System.Timers.Timer(24 * 60 * 60 * 1000);
#endif
        _timer.Elapsed += Digest;
        _timer.Enabled = true;
        _timer.Start();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Run the digest in a task.
    /// </summary>
    private void Digest(object? sender, ElapsedEventArgs e)
    {
        Logger.LogInformation("Started processing Digest");
        var digestTask = Task.Run(DigestAsync);
        // an error should not shut down the bot
        try
        {
            digestTask.Wait();
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Error in DigestAsync");
        }

        Logger.LogInformation("Digest complete");
    }

    /// <summary>
    /// Parses all categories and sends updates.
    /// </summary>
    private async Task DigestAsync()
    {
        // Get all categories and get one Category object for each distinct category (we want to avoid downloading the same category twice)
        var categoricalAlerts = await Data.GetAllAlertsAsync<ChannelCategoryAlert>();
        var channelCategoryAlerts = categoricalAlerts as ChannelCategoryAlert[] ?? categoricalAlerts.ToArray();
        IEnumerable<Task<Category>> categoriesTasks = channelCategoryAlerts.Select(alert => alert.CategoryId).Distinct()
            .Select(async x => await GetCategoryFromId(x));
        var categories = await Task.WhenAll(categoriesTasks);
        // join with the alerts to match alert destination with alert contents
        var joined = channelCategoryAlerts.Join(categories, alert => alert.CategoryId, category => category.CategoryId,
            (alert, category) => new Tuple<ChannelCategoryAlert, Category>(alert, category));

        foreach (var ac in joined)
        {
            var (alert, category) = ac;
            category.Resolved.ForEach(x =>
            {
                var tup = new Tuple<ChannelCategoryAlert, AlertQuestion>(alert, x);
                Task.Run(() => CreateAlertMessageAndSendAsync(tup, AlertKind.Resolved));
            });
            category.Ambiguous.ForEach(x =>
            {
                var tup = new Tuple<ChannelCategoryAlert, AlertQuestion>(alert, x);
                Task.Run(() => CreateAlertMessageAndSendAsync(tup, AlertKind.Ambiguous));
            });
            category.News.ForEach(x =>
            {
                var tup = new Tuple<ChannelCategoryAlert, AlertQuestion>(alert, x);
                Task.Run(() => CreateAlertMessageAndSendAsync(tup, AlertKind.New));
            });
            category.DaySwing.ForEach(x =>
            {
                var tup = new Tuple<ChannelCategoryAlert, AlertQuestion>(alert, x);
                Task.Run(() => CreateAlertMessageAndSendAsync(tup, AlertKind.DaySwing));
            });
        }
    }

    /// <summary>
    /// Downloads all questions from the category by searching with the category id in the order of last activity.
    /// Parses them and adds them to the Category class which keeps only the interesting ones.
    /// </summary>
    /// <param name="categoryId">Id of the category to be recovered.</param>
    /// <returns>Task whose result is the Category object containing only interesting questions.</returns>
    private async Task<Category> GetCategoryFromId(string categoryId)
    {
        const int limit = 100; // the maximum the API supports
        // this way new questions are on the top
        var link =
            $"https://www.metaculus.com/api2/questions/?search=cat:{categoryId}&limit={limit}&order_by=-last_activity_time";
        using var client = new HttpClient();
        var responseString = await client.GetStringAsync(link);
        var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(responseString);
        var category = new Category(Configuration.GetValue<double>("DaySwingThreshold")) {CategoryId = categoryId};
        do
        {
            // go through the search results
            foreach (var dynamicQuestion in dynamicResponse?.results!)
            {
                if (dynamicQuestion.type != "forecast") continue;
                // stop when the updates are more than a day old
                if (DateTime.Now - DateTime.Parse((string) dynamicQuestion.last_activity_time) >
                    TimeSpan.FromDays(1))
                    goto superbreak;
                try
                {
                    var q = new AlertQuestion(dynamicQuestion);
                    category.AddQuestion(q);
                }
                catch (Exception e)
                {
                    // bad things can happen while parsing one question and we want to continue with the rest
                    Logger.LogError(e, "Error in parsing question in category: {CategoryId}", categoryId);
                }
            }

            // there is no next page
            if (dynamicResponse.next == null || dynamicResponse.next.Equals("")) break;
            // get the next page 
            responseString = await client.GetStringAsync(dynamicResponse.next);
            dynamicResponse = JsonConvert.DeserializeObject<dynamic>(responseString);
        } while (true);

        superbreak:

        return category;
    }
}
