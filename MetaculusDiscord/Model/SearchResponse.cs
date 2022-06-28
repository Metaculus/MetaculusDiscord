namespace MetaculusDiscord.Model;

/// <summary>
/// Used for creating the response to a search request.
/// </summary>
public class SearchResponse
{
    private const int MaxResponses = 5;
    public SearchResultQuestion[] Questions { get; } = new SearchResultQuestion[5];
    public int Count { get; private set; }

    /// <summary>
    /// Adds a question to the collection.
    /// </summary>
    /// <returns>whether the Response has more capacity</returns>
    public bool AddQuestion(SearchResultQuestion q)
    {
        if (Count >= MaxResponses) return false;
        Questions[Count++] = q;
        return Count < MaxResponses;
    }
}

/// <summary>
/// Stores the long version of links in a search response,
/// so that when the user selects a search query it is responded to with the full link.
/// <remarks> todo: this part of the model is a bit strange and is a good candidate for refactor
/// </remarks>
/// </summary>
public record ResponseLinks
{
    private const int MaxResponses = 5;

    public ResponseLinks(ulong id, IEnumerable<string> pagePaths)
    {
        Id = id;
        var i = 0;
        // stores up to 5 links
        foreach (var link in pagePaths)
        {
            Links[i] = $"https://www.metaculus.com{link}";
            if (++i == Links.Length) break;
        }
    }

    public ulong Id { get; }
    public string[] Links { get; } = new string[MaxResponses];
}