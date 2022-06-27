using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using MetaculusDiscord.Model;
using MetaculusDiscord.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MetaculusDiscord.Services;

public abstract class AlertDiscordClientService : DiscordClientService
{
    public Data.Data Data { get; set; }
    public IConfiguration Configuration { get; set; }
    public ILogger<AlertDiscordClientService> Logger { get; set; }
    public AlertDiscordClientService(DiscordSocketClient client, ILogger<AlertDiscordClientService> logger,Data.Data data, IConfiguration configuration) : base(client, logger)
    {
        Data = data;
        Configuration= configuration;
        Logger = logger;
    }
    /// <summary>
    ///     Sends a message either to channel or DM depending on the alert type.
    /// </summary>
    /// <param name="message">Message to be sent</param>
    /// <param name="alert">Alert whose target is sent the message.</param>
    /// <typeparam name="TAlert">QuestionAlert</typeparam>
    private async Task SendAlertMessageAsync<TAlert>(string message, TAlert alert) where TAlert : Alert 
    {
        if (alert is UserQuestionAlert userAlert)
        {
            var target = await Client.GetUserAsync(userAlert.UserId);
            await target.SendMessageAsync(message);
        }
        else if (alert is ChannelQuestionAlert channelAlert )
        {
            if (await Client.GetChannelAsync(channelAlert.ChannelId) is ITextChannel target)
                await target.SendMessageAsync(message);
        }
        else
        {
            if (alert is ChannelCategoryAlert channelCategoryAlert)
            {
                if (await Client.GetChannelAsync(channelCategoryAlert.ChannelId) is ITextChannel target)
                    foreach (var line in message.Split('\n')) // split to have a link on a separate line 
                        await target.SendMessageAsync(line);
            }
        }
    }

    protected enum AlertKind
    {
        Resolved,
        Ambiguous,
        SixHourSwing,
        DaySwing,
        New
    }

    /// <summary>
    ///     Create message for an alert depending on its kind and send it.
    /// </summary>
    protected async Task CreateAlertMessageAndSendAsync<T>(Tuple<T, AlertQuestion> t, AlertKind kind)
        where T : Alert 
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
            case AlertKind.New:
                message = $"New question: {question.Title} \n{question.ShortUrl()}";
                
                break; 
            default:
                message = "";
                break;
        }

        await SendAlertMessageAsync(message, alert);
    }
}