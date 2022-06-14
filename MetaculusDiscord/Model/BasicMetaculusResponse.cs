namespace MetaculusDiscord.Model;

public class BasicMetaculusResponse : BasicQuestionResponse
{
    public string[] Links { get; set; } = new string[5];

    public BasicMetaculusResponse(ulong id, ICollection<string> endings)
    {
        Id = id;
        int i = 0;
        foreach (var link in endings)
        {
            Links[i] = $"https://www.metaculus.com{link}";
            if (++i == Links.Length) break;
        }
        
    }

}