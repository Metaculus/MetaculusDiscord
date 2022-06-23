namespace MetaculusDiscord.Model;

public class ResponseLinks
{
    public ulong Id { get; set; }
    public string[] Links { get; } = new string[5];

    public ResponseLinks(ulong id, IEnumerable<string> pagePaths)
    {
        Id = id;
        var i = 0;
        foreach (var link in pagePaths)
        {
            Links[i] = $"https://www.metaculus.com{link}";
            if (++i == Links.Length) break;
        }
    }
}