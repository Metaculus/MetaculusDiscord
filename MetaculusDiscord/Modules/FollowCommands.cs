using System.Diagnostics.Eventing.Reader;
using Discord.Commands;
using MetaculusDiscord.Model;
using MetaculusDiscord.Utils;

namespace MetaculusDiscord.Modules;

public class FollowCommands : BotModuleBase
{
    public FollowCommands(Data.Data data) : base(data)
    {
    }

    [Command("followcategory")]
    public async Task FollowCategory(string categoryId)
    {
        if (!(await ApiUtils.IsCategoryValid(categoryId))) 
        {
            await Context.Channel.SendMessageAsync("Invalid category id");
        }
        else
        {
            var categoryAlert = new ChannelCategoryAlert()
            {
                CategoryId = categoryId,
                ChannelId = Context.Channel.Id,
            };
            if (await Data.TryAddAlertAsync(categoryAlert));
                await Context.Channel.SendMessageAsync("Now following category " + categoryId);
            
        }
    }

    [Command("unfollowcategory")]
    public async Task UnfollowCategory(string categoryId)
    {
        ulong channelId = Context.Channel.Id;
       var alert = new ChannelCategoryAlert()
        {
            CategoryId = categoryId,
            ChannelId = channelId,
        };
        if (await Data.TryRemoveAlertAsync(alert))
            await Context.Channel.SendMessageAsync("No longer following category " + categoryId);
        else
            await Context.Channel.SendMessageAsync("This channel is not following category " + categoryId);
    }
}