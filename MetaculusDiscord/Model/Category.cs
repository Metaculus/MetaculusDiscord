using MetaculusDiscord.Model;

namespace MetaculusDiscord.Services;

public class Category
{
    private readonly double _daySwingThreshold;

    public Category(double daySwingThreshold)
    {
        this._daySwingThreshold = daySwingThreshold;
    }

    public string CategoryId { get; set; }
    public List<AlertQuestion> News { get; } = new List<AlertQuestion>();
    public List<AlertQuestion> Resolved { get; } = new List<AlertQuestion>();
    public List<AlertQuestion> Ambiguous { get; } = new List<AlertQuestion>();
    public List<AlertQuestion> DaySwing { get; } = new List<AlertQuestion>();

    public void AddQuestion(AlertQuestion question)
    {
        if (question.Resolved == null)
        {
            Ambiguous.Add(question);
        }
        else if (question.Resolved!.Value)
        {
            Resolved.Add(question);
        }
        else if (question.PublishTime > DateTime.Now - TimeSpan.FromDays(1))
        {
            News.Add(question);
        }
        else if (question.IsDaySwing(_daySwingThreshold))
        {
            DaySwing.Add(question);
        }
    }
}