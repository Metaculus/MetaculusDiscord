namespace MetaculusDiscord.Model;

public class MetaculusSearchResponse
{
    private const int MaxResponses = 5;
    public SearchResultQuestion[] Questions { get; } = new SearchResultQuestion[5];
    public int Count { get; private set; } = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <returns>whether the Response has more capacity</returns>
    public bool AddQuestion(SearchResultQuestion q)
    {
        if (Count >= MaxResponses) return false;
        Questions[Count++] = q;
        return Count < MaxResponses;
    }
}