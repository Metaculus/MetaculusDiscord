using Discord;
using Discord.Commands;
using MetaculusDiscord.Model;

namespace MetaculusDiscord.Modules;

public class AlertCommands : BotModuleBase
{
    public AlertCommands(Data.Data data) : base(data)
    {
    }

    [Command("alert")]
    public async Task SetUserAlert(int question)
    {
        var userId = Context.Message.Author.Id;
        var alert = new UserQuestionAlert {UserId = userId, QuestionId = question};
        if (await Data.TryAddAlertAsync(alert))
            await Context.Message.Author.SendMessageAsync($"Alert for question {question} set");
        else
            await Context.Message.Author.SendMessageAsync($"Error: Alert for question {question} already set");
    }

    [Command("channelalert")]
    public async Task SetChannelAlert(int question)
    {
        if (Context.Channel.GetChannelType() == ChannelType.DM)
        {
            await Context.Message.Author.SendMessageAsync("Error: Channel alert can only be set on a server.");
            return;
        }

        var channelId = Context.Channel.Id;

        var alert = new ChannelQuestionAlert {ChannelId = channelId, QuestionId = question};
        if (await Data.TryAddAlertAsync(alert))
            await Context.Channel.SendMessageAsync($"Alert for question {question} set");
        else
            await Context.Channel.SendMessageAsync($"Error: Alert for question {question} already set");
    }

    [Command("unalert")]
    public async Task UnsetUserAlert(int question)
    {
        var userId = Context.Message.Author.Id;
        var alert = new UserQuestionAlert {UserId = userId, QuestionId = question};
        if (await Data.TryRemoveAlertAsync(alert))
            await Context.Message.Author.SendMessageAsync($"Alert for question {question} unset");
        else
            await Context.Message.Author.SendMessageAsync($"Error: No alert for question {question} set");
    }

    [Command("unchannelalert")]
    public async Task UnsetChannelAlert(int question)
    {
        if (Context.Channel.GetChannelType() == ChannelType.DM)
        {
            await Context.Message.Author.SendMessageAsync("Error: Channel alert can only be set on a server.");
            return;
        }

        var channelId = Context.Channel.Id;
        var alert = new ChannelQuestionAlert {ChannelId = channelId, QuestionId = question};
        if (await Data.TryRemoveAlertAsync(alert))
            await Context.Channel.SendMessageAsync($"Channel alert for question {question} unset");
        else
            await Context.Message.Channel.SendMessageAsync($"Error: No channel alert for question {question} set");
    }
}