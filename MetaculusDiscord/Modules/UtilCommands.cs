using System.Text;
using System.Text.Json.Nodes;
using Discord.Commands;
using Newtonsoft.Json;

namespace MetaculusDiscord.Modules;

/// <summary>
/// Commands that don't interact with the bot state.
/// </summary>
public class UtilCommands : BotModuleBase
{
    public UtilCommands(Data.Data data) : base(data)
    {
    }

    [Command("help")]
    public async Task Help()
    {
        await Context.Channel.SendMessageAsync(
            @"type `/metaculus <query>` to search for a question.
put a `:warning:` emoji on a message with a link to get notified of its updates

For moderators:
`!metac channelalert <question_id>` to set a channel to be notified question updates
`!metac unchannelalert <question_id>` to remove a channel alert

`!metac listcategories` to list all categories
`!metac followcategory <category_id>` to follow a category in this channel.
`!metac unfollowcategory <category_id>` to unfollow a category 

(you can also use `!metac alert <id>` and `!metac unalert <id>` instead of the warning emoji)
"
        );
    }

    /// <summary> 
    /// Gets the list of categories from: https://www.metaculus.com/api2/categories/?limit=999
    /// parses it and sends it.
    /// </summary>
    [Command("listcategories")]
    public async Task ListCategories()
    {
        using var client = new HttpClient();
        var json = await client.GetStringAsync("https://www.metaculus.com/api2/categories/?limit=999");
        var dynamicResults = JsonConvert.DeserializeObject<dynamic>(json);
        StringBuilder sb = new();
        sb.Append("id : name \n");
        foreach (var category in dynamicResults?.results!)
        {
            sb.Append($"{category.id} : {category.short_name} \n");
            // Discord limits messages to 2000 characters.
            if (sb.Length > 1800)
            {
                await Context.Channel.SendMessageAsync(sb.ToString());
                sb.Clear();
            }
        }

        await Context.Channel.SendMessageAsync(sb.ToString());
    }
}
