using System.Net;
using System.Runtime.InteropServices;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace MetaculusDiscord.Modules;
public class RandomCommandsModule : ModuleBase<SocketCommandContext>
{
    private static readonly HttpClient _httpClient = new HttpClient();

    private static Task<string?> GetStringResponse(string url)
    {
        try
        {
            return _httpClient.GetStringAsync(url)!;
        }
        catch (Exception e)
        {
            return Task.Run(() => (string?)null);
        }
    }

    private const int QueryResults = 5;
    [Command("search")]
    [Alias("s")]
    public async Task SearchAsync(params string[] querys)
    {
        var query = String.Join(" ",querys);
        string requestUrl =
            $"https://www.metaculus.com/api2/questions/?order_by=-rank&search={string.Join(" ", query)}";
        string? response = await GetStringResponse(requestUrl);

        if (response is null)
        {
            await Context.Message.ReplyAsync($"No results for query: {query}");
        }
        else
        {
           dynamic? root = JsonConvert.DeserializeObject<dynamic>(response);
           
           dynamic results = root.results;
           string[] names = new string[QueryResults];
           string[] links = new string[QueryResults];
            for (int i = 0; (i < results.Count && i<QueryResults); i++)
            {
                var queryResult = results[i];
                
                await Context.Channel.SendMessageAsync($"Result {i + 1}: {queryResult.title} https://www.metaculus.com{queryResult.page_url}");
                // Context.Channel.SendMessageAsync(embed: CreateEmbed())
            }
        }
    }

    public Embed CreateEmbed()
    {
        return null;
    }
    
    
    
    [Command("a")]
    public async Task A()
    {
        await Context.Channel.SendMessageAsync(("aaaaaaaaa"));
    }
    
}