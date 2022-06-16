using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MetaculusDiscord.Model;
using MetaculusDiscord.Utils;
using Newtonsoft.Json;

namespace MetaculusDiscord.Modules;

public class Search
{
    public static async Task<string> CreateResponseText(string query, MetaculusSearchResponse? response)
    {
        if (response is null)
            return $"No results for query: {query}";
        if (response.Index == 0)
            return $"No results for query: {query}";

        if (response.Index == 1)
            return $"https://www.metaculus.com{response.Questions[0].PageUrl}";

        var replyBuilder = new StringBuilder();
        replyBuilder.Append("Results:    *press an emote for details*\n");
        var i = 0;
        for (; i < response.Index; i++)
        {
            var question = response.Questions[i];

            replyBuilder.Append(
                $"{EmotesUtils.EmojiDict[i + 1]}: **{question.Title}**, Published: {question.PublishTime.ToString("yyyy-MM-dd")}\n");
        }

        return replyBuilder.ToString();
    }

    public class SearchCommands : BotModuleBase
    {
        private const int QueryResults = 5;


        [Command("search")]
        [Alias("s")]
        public async Task SearchCommand(params string[] searchStrings)
        {
            var query = string.Join(" ", searchStrings);
            // string? response = await SearchUtils.GetStringResponseAsync(requestUrl);
            var response = await SearchAsync(query);
            var replyText = await CreateResponseText(query, response);
            var predictionReply = await Context.Channel.SendMessageAsync(replyText);
            if (response is null) return;
            if (response.Index > 1) EmotesUtils.Decorate(predictionReply, response.Index);
            var messageLinks = new ResponseLinks(predictionReply.Id, response.Questions.Select(q => q.PageUrl));
            Data.StoreLinks(messageLinks);
        }

        public SearchCommands(Data.Data data) : base(data)
        {
        }
    }


    public class SearchSlash : BotInteractionModuleBase
    {
        [SlashCommand("metaculus", "")]
        public async Task SearchCommand(string query)
        {   // there are only 3 seconds to respond, so first we have to
            await RespondAsync($"searching {query}...");
            var response = await SearchAsync(query);
            var replyText = await CreateResponseText(query, response);

            var reply = await Context.Interaction.FollowupAsync(replyText);
            if (response is null) return;
            if (response.Index > 1) EmotesUtils.Decorate(reply, response.Index);
            var messageLinks = new ResponseLinks(reply.Id, response.Questions.Select(q => q.PageUrl));
            Data.StoreLinks(messageLinks);
            if (response.Index > 1)
                EmotesUtils.Decorate(reply, response.Index);
        }


        public SearchSlash(Data.Data data) : base(data)
        {
        }
    }

    private static readonly HttpClient HttpClient = new();

    public static Task<string?> GetStringResponseAsync(string url)
    {
        try
        {
            return HttpClient.GetStringAsync(url)!;
        }
        catch (Exception e)
        {
            return Task.Run(() => (string?) null);
        }
    }

    public static async Task<MetaculusSearchResponse?> SearchAsync(string query)
    {
        var requestUrl = $"https://www.metaculus.com/api2/questions/?order_by=-rank&search={query}";
        var jsonString = await GetStringResponseAsync(requestUrl);
        if (jsonString is null) return null;
        var root = JsonConvert.DeserializeObject<dynamic>(jsonString);
        var results = root?.results;
        if (results is null || results.Count == 0) return null;
        var response = new MetaculusSearchResponse();
        for (var i = 0; i < results.Count; i++)
        {
            var dynamicQuestion = results[i];
            string publishTime = dynamicQuestion.publish_time;
            var publishedDate = DateTime.Parse(publishTime).Date;
            int id = dynamicQuestion.id;
            string title = dynamicQuestion.title;
            string pageUrl = dynamicQuestion.page_url;
            var q = new MetaculusQuestion(id, title, pageUrl, publishedDate);
            if (!response.AddQuestion(ref q)) break;
        }

        return response;
    }
}