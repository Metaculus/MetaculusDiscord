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
    public static string CreateResponseText(string query, MetaculusSearchResponse? response)
    {
        if (response is null)
            return $"No results for query: {query}";
        if (response.Count == 0)
            return $"No results for query: {query}";

        if (response.Count == 1)
            return $"https://www.metaculus.com{response.Questions[0].PageUrl}";

        var replyBuilder = new StringBuilder();
        replyBuilder.Append("Results:    *press an emote for details*\n");
        var i = 0;
        for (; i < response.Count; i++)
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
            var response = await SearchAsync(query);
            var replyText = CreateResponseText(query, response);
            var predictionReply = await Context.Channel.SendMessageAsync(replyText);
            if (response is null) return;
            if (response.Count > 1) EmotesUtils.Decorate(predictionReply, response.Count);
            var messageLinks = new ResponseLinks(predictionReply.Id, response.Questions.Select(q => q.PageUrl));
            Data.TryAddResponse(messageLinks);
        }

        public SearchCommands(Data.Data data) : base(data)
        {
        }
    }


    public class SearchSlash : BotInteractionModuleBase
    {
        [SlashCommand("metaculus", "")]
        public async Task SearchCommand(string query)
        {
            // there are only 3 seconds to respond, so first we have to
            await RespondAsync($"searching {query}...");
            var response = await SearchAsync(query);
            var replyText = CreateResponseText(query, response);

            var reply = await Context.Interaction.FollowupAsync(replyText);
            if (response is null) return;
            if (response.Count > 1) EmotesUtils.Decorate(reply, response.Count);
            var messageLinks = new ResponseLinks(reply.Id, response.Questions.Select(q => q.PageUrl));
            Data.TryAddResponse(messageLinks);
            if (response.Count > 1)
                EmotesUtils.Decorate(reply, response.Count);
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
        catch (Exception)
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
        for (var i = 0; i < results?.Count; i++)
        {
            var dynamicQuestion = results?[i];
            bool isForecast;
            try
            {
                isForecast = dynamicQuestion?.type == "forecast";
            }
            catch
            {
                continue;
            }

            if (!isForecast) continue; // we want only forecasts
            var q = new SearchResultQuestion(dynamicQuestion);
            if (!response.AddQuestion(q)) break;
        }

        if (response.Count == 0) return null;

        return response;
    }
}