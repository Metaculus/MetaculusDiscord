using System.Text;
using System.Text.Json.Nodes;
using Discord.Commands;
using Newtonsoft.Json;

namespace MetaculusDiscord.Modules;

public class UtilCommands : BotModuleBase
{
    public UtilCommands(Data.Data data) : base(data)
    {
    }

    [Command("help")]
    public async Task Help()
    {
        await Context.Channel.SendMessageAsync(
            "The bot currently supports the following commands: \n   `!mc search <query>`, `search` can be replaced by `s`" +
            "\n" +
            "to select from the results, press the reaction with the corresponding number");
        //todo update this
    }
    
    [Command("listcategories")]
    public async Task ListCategories()
    {
        // get this: https://www.metaculus.com/api2/categories/?limit=999
        // and parse it
        using HttpClient client = new HttpClient();
        string json = await client.GetStringAsync( "https://www.metaculus.com/api2/categories/?limit=999");
        var obj = JsonConvert.DeserializeObject<dynamic>(json);
        StringBuilder sb = new();
        sb.Append("id : name \n");
        foreach (var category in obj.results)
        {
            sb.Append($"{category.id} : {category.short_name} \n");
            if (sb.Length > 1800)
            {
                await Context.Channel.SendMessageAsync(sb.ToString());
                sb.Clear();
            }
                
        }
        await Context.Channel.SendMessageAsync(sb.ToString());

    }
}