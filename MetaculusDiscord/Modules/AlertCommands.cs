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
    public async Task SetUserAlert(int questionId)
    {
        var userId = Context.Message.Author.Id;
        var alert = new UserQuestionAlert {UserId = userId, QuestionId = questionId};
        if (await Data.TryAddAlertAsync(alert))
            await Context.Message.Author.SendMessageAsync($"Alert for question {questionId} set");
        else
            await Context.Message.Author.SendMessageAsync($"Error: Alert for question {questionId} already set");
    }

    [Command("channelalert")]
    public async Task SetChannelAlert(int questionId)
    {
        if (Context.Channel.GetChannelType() == ChannelType.DM)
        {
            await Context.Message.Author.SendMessageAsync("Error: Channel alert can only be set on a server.");
            return;
        }

        var channelId = Context.Channel.Id;

        var alert = new ChannelQuestionAlert {ChannelId = channelId, QuestionId = questionId};
        if (await Data.TryAddAlertAsync(alert))
            await Context.Channel.SendMessageAsync($"Alert for question {questionId} set");
        else
            await Context.Channel.SendMessageAsync($"Error: Alert for question {questionId} already set");
    }

    [Command("unalert")]
    public async Task UnsetUserAlert(int questionId)
    {
        var userId = Context.Message.Author.Id;
        var alert = new UserQuestionAlert {UserId = userId, QuestionId = questionId};
        if (await Data.TryRemoveAlertAsync(alert))
            await Context.Message.Author.SendMessageAsync($"Alert for question {questionId} unset");
        else
            await Context.Message.Author.SendMessageAsync($"Error: No alert for question {questionId} set");
    }

    [Command("unchannelalert")]
    public async Task UnsetChannelAlert(int questionId)
    {
        if (Context.Channel.GetChannelType() == ChannelType.DM)
        {
            await Context.Message.Author.SendMessageAsync("Error: Channel alert can only be set on a server.");
            return;
        }

        var channelId = Context.Channel.Id;
        var alert = new ChannelQuestionAlert {ChannelId = channelId, QuestionId = questionId};
        if (await Data.TryRemoveAlertAsync(alert))
            await Context.Channel.SendMessageAsync($"Channel alert for question {questionId} unset");
        else
            await Context.Message.Channel.SendMessageAsync($"Error: No channel alert for question {questionId} set");
    }
}