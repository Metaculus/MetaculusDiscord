using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Discord;
using Discord.Commands;
using MetaculusDiscord.Model;
using Newtonsoft.Json;

namespace MetaculusDiscord.Modules;
public class SearchCommands : BotModuleBase
{
    
    private static Task<string?> GetStringResponse(string url)
    {
        try
        {
            return HttpClient.GetStringAsync(url)!;
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
           if (results.Count == 0)
           { 
               await Context.Message.ReplyAsync($"No results for query: {query}");
               return;
           }
           if (results.Count == 1)
           {
               PostLink($"https://www.metaculus.com{(string) results[0].page_url}");
               return;
           }
           var newMessageParts = new List<string>();
           for (int i = 0; (i < results.Count && i < QueryResults); i++)
           {
               var queryResult = results[i];

               newMessageParts.Add($"Result {i + 1}: {queryResult.title}, Published: {queryResult.publish_time}");
               links[i] = queryResult.page_url;
           }
                 

           var newMessageString = string.Join("\n", newMessageParts);
           var predictionReply = await Context.Channel.SendMessageAsync(newMessageString);
           var x = new BasicMetaculusResponse(predictionReply.Id, links);
           Data.AddResponse(x);
           var emojis = Utils.EmotesUtils.GetEmojiNumbersDict();
           for (int i = 1; i < newMessageParts.Count+1; i++)
           {
               await predictionReply.AddReactionAsync(emojis[i]);
           }
            
        }
    }

    private void PostLink(string s)
    {
        Context.Channel.SendMessageAsync(s);
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

    public SearchCommands(Data.Data data) : base(data)
    {
    }
}