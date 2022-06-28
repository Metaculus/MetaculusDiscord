namespace MetaculusDiscord.Model;

/// <summary>
/// Object that holds questions of a category separated into lists that specify why they are noteworthy.
/// </summary>
public class Category
{
    private readonly double _daySwingThreshold;

    public Category(double daySwingThreshold)
    {
        _daySwingThreshold = daySwingThreshold;
    }

    public string CategoryId { get; init; } = "";
    public List<AlertQuestion> News { get; } = new();
    public List<AlertQuestion> Resolved { get; } = new();
    public List<AlertQuestion> Ambiguous { get; } = new();
    public List<AlertQuestion> DaySwing { get; } = new();

    /// <summary>
    /// Question is added to the appropriate list if it is interesting (something happened with it).
    /// </summary>
    /// <param name="question"></param>
    public void AddQuestion(AlertQuestion question)
    {
        if (!question.Resolved.HasValue)
            Ambiguous.Add(question);
        else if (question.Resolved!.Value)
            Resolved.Add(question);
        else if (question.PublishTime > DateTime.Now - TimeSpan.FromDays(1))
            News.Add(question);
        else if (question.IsDaySwing(_daySwingThreshold)) DaySwing.Add(question);
    }
}