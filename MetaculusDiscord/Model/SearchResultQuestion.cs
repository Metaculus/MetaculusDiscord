namespace MetaculusDiscord.Model;

public class Question
{
    public int Id { get; set; }
}

public class SearchResultQuestion : Question
{
    public enum QuestionType
    {
        Continuous,
        Binary,
        Date
    }

    public string Title { get; set; }
    public string PageUrl { get; set; }
    public DateTime PublishTime { get; set; }
    public bool? Resolved { get; set; }

    public SearchResultQuestion(dynamic dynamicQuestion)
    {
        string publishTime = dynamicQuestion.publish_time;
        PublishTime = DateTime.Parse(publishTime).Date;
        Id = dynamicQuestion.id;
        Title = dynamicQuestion.title;
        PageUrl = dynamicQuestion.page_url;
    }
}