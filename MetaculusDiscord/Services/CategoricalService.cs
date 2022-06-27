using System.Diagnostics;
using System.Timers;
using Discord.WebSocket;
using MetaculusDiscord.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace MetaculusDiscord.Services;

public class CategoricalService : AlertDiscordClientService
{
    public CategoricalService(DiscordSocketClient client, ILogger<CategoricalService> logger, Data.Data data,
        IConfiguration configuration) : base(client,
        logger, data, configuration)
    {
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
        timer.Enabled = true;
        timer.Start();
    }

    private void Digest(object? sender, ElapsedEventArgs e)
    {
        Logger.LogInformation("Started processing Digest.");
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

        Logger.LogInformation("Digest complete.");
    }


    private async Task DigestAsync()
    {
        var categoricalAlerts = await Data.GetAllAlertsAsync<ChannelCategoryAlert>();

        var channelCategoryAlerts = categoricalAlerts as ChannelCategoryAlert[] ?? categoricalAlerts.ToArray();
        IEnumerable<Task<Category>> categoriesTasks = channelCategoryAlerts.Select(alert => alert.CategoryId).Distinct().Select(async x=> await CategoryIdToCategoryObject(x));
        var categories = await Task.WhenAll(categoriesTasks);
        var joined = channelCategoryAlerts.Join(categories, alert => alert.CategoryId, category => category.CategoryId,
            (alert, category) => new Tuple<ChannelCategoryAlert,Category>(alert, category));

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

    private async Task<Category> CategoryIdToCategoryObject(string categoryId)
    {
        int limit = 100; // the maximum the API supports
        // this way new questions are on the top
        string link =
            $"https://www.metaculus.com/api2/questions/?search=cat:{categoryId}&limit={limit}&order_by=-last_activity_time";
        using HttpClient client = new HttpClient();
        var responseString = await client.GetStringAsync(link);

        var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(responseString);
        var category = new Category(Configuration.GetValue<double>("DaySwingThreshold")) {CategoryId = categoryId};
        do
        {
            foreach (var dynamicQuestion in dynamicResponse?.results!)
            {
                if (dynamicQuestion.type == "forecast")
                {
                    if (DateTime.Now - DateTime.Parse((string) dynamicQuestion.last_activity_time) >
                        TimeSpan.FromDays(1))
                        goto superbreak;
                    try
                    {
                        var q = new AlertQuestion(dynamicQuestion);
                        category.AddQuestion(q);
                    } catch (Exception e)
                    { // many bad things can happen while parsing the question
                        Logger.LogError(e, "Error in parsing question");
                    }
                }
            }

            if (dynamicResponse.next == null || dynamicResponse.next.Equals("")) break;
            responseString = await client.GetStringAsync(dynamicResponse.next);
            dynamicResponse = JsonConvert.DeserializeObject<dynamic>(responseString);
        } while (true);

        superbreak:
        return category;
    }

}