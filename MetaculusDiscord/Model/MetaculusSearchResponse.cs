namespace MetaculusDiscord.Model;

public class MetaculusSearchResponse
{
    private const int MaxResponses = 5;
    public MetaculusQuestion[] Questions { get; } = new MetaculusQuestion[5];
    public int Index { get; private set; } = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <returns>whether the Response has more capacity</returns>
    public bool AddQuestion(ref MetaculusQuestion q)
    {
        if (Index >= MaxResponses) return false;
        Questions[Index++] = q;
        return Index < MaxResponses;
    }
}