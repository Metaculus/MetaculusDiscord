using System.Text;
using Discord.Commands;
using Discord.Interactions;
using MetaculusDiscord.Model;
using MetaculusDiscord.Utils;
using Newtonsoft.Json;

namespace MetaculusDiscord.Modules;

/// <summary>
/// Superclass for both search modules which provides common static functionality.
/// </summary>
public class Search
{
    /// <summary>
    /// Creates message text to be sent from the search results.
    /// </summary>
    /// <param name="query">What was searched.</param>
    /// <param name="response">What was found.</param>
    /// <returns>text of the message</returns>
    private static string CreateResponseText(string query, SearchResponse? response)
    {
        if (response is null)
            return $"No results for query: {query}";
        if (response.Count == 0)
            return $"No results for query: {query}";
        // directly return the single result
        if (response.Count == 1)
            return $"https://www.metaculus.com{response.Questions[0].PageUrl}";

        var replyBuilder = new StringBuilder();
        replyBuilder.Append("Results:    *press an emote for details*\n");
        var i = 0;
        for (; i < response.Count; i++)
        {
            var question = response.Questions[i];
            // using emoji for numbers
            replyBuilder.Append(
                $"{EmotesUtils.NumberEmoji[i + 1]}: **{question.Title}**, Published: {question.PublishTime.ToString("yyyy-MM-dd")}\n");
        }

        return replyBuilder.ToString();
    }

    /// <summary>
    /// Queries the API and parses the response to an extent so it can be printed.
    /// </summary>
    /// <param name="query">query string</param>
    /// <returns>Task whose result are the parsed questions.</returns>
    private static async Task<SearchResponse?> SearchAsync(string query)
    {
        var requestUrl = $"https://www.metaculus.com/api2/questions/?order_by=-rank&search={query}";
        using HttpClient client = new();
        string? jsonString;
        try
        {
            jsonString = await client.GetStringAsync(requestUrl);
        }
        // catch exception when the request fails
        catch (Exception)
        {
            jsonString = null;
        }

        if (jsonString is null) return null; // there is nothing to parse
        var root = JsonConvert.DeserializeObject<dynamic>(jsonString);
        var results = root?.results;
        if (results is null || results.Count == 0) return null;
        var response = new SearchResponse();
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

        // if we got no matching forecasts, return null
        return response.Count == 0 ? null : response;
    }

    /// <summary>
    /// Module representing the regular command that performs a search.
    /// </summary>
    public class SearchCommands : BotModuleBase
    {
        public SearchCommands(Data.Data data) : base(data)
        {
        }


        [Command("search")]
        [Alias("s")]
        public async Task SearchCommand(params string[] searchStrings)
        {
            var query = string.Join(" ", searchStrings);
            var response = await SearchAsync(query);
            var replyText = CreateResponseText(query, response);
            var predictionReply = await Context.Channel.SendMessageAsync(replyText);
            if (response is null) return;
            var messageLinks = new ResponseLinks(predictionReply.Id, response.Questions.Select(q => q.PageUrl));
            Data.TryAddResponse(messageLinks);
            if (response.Count > 1) EmotesUtils.NumberDecorate(predictionReply, response.Count);
        }
    }

    /// <summary>
    /// Module with slash command that performs a search.
    /// </summary>
    public class SearchSlash : BotInteractionModuleBase
    {
        public SearchSlash(Data.Data data) : base(data)
        {
        }

        [SlashCommand("metaculus", "")]
        public async Task SearchCommand(string query)
        {
            // Discord requires a response in 3 seconds, but that's often not enough time to produce the full response
            await RespondAsync($"searching {query}...");
            var response = await SearchAsync(query);
            var replyText = CreateResponseText(query, response);

            var reply = await Context.Interaction.FollowupAsync(replyText);
            if (response is null) return;
            var messageLinks = new ResponseLinks(reply.Id, response.Questions.Select(q => q.PageUrl));
            Data.TryAddResponse(messageLinks);
            if (response.Count > 1) EmotesUtils.NumberDecorate(reply, response.Count);
        }
    }
}