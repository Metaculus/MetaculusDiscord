namespace MetaculusDiscord.Model;
enum QuestionType { Continuous, Binary, Date}
public class MetaculusQuestion
{
    
    public int Id { get; }
    public string Title { get; }
    public string PageUrl { get; }
    public DateTime PublishTime { get; }
    public bool? Resolved { get; }

    public MetaculusQuestion(int id, string title, string pageUrl, DateTime publishTime, bool? resolved = null)
    {
        Id = id;
        Title = title;
        PageUrl = pageUrl;
        Resolved = resolved;
        PublishTime = publishTime;
    }
}