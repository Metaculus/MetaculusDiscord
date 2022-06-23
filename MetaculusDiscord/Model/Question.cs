namespace MetaculusDiscord.Model;
/// <summary>
/// Representation of a Metaculus question.
/// </summary>
public class Question
{
    public enum QuestionType
    {
        Continuous,
        Binary,
        Date
    }
    public long Id { get; set; }
    public string Title { get; set; } = "";
    public QuestionType Type { get; set; }
    
}
/// <summary>
/// Question representation used for showing the user search results.
/// </summary>
public class SearchResultQuestion : Question
{
    public string PageUrl { get; set; }
    public DateTime PublishTime { get; set; }

    public SearchResultQuestion(dynamic dynamicQuestion) 
    {
        Id = dynamicQuestion.id;
        Title = dynamicQuestion.title;
        string publishTime = dynamicQuestion.publish_time;
        PublishTime = DateTime.Parse(publishTime).Date;
        PageUrl = dynamicQuestion.page_url;
    }
}
/// <summary>
/// Question representation used for sending alerts.
/// </summary>
public class AlertQuestion : Question
{
    public double Value { get; set; } 
    public double DayOldValue { get; set; }
    public double SixHoursOldValue { get; set; }
    public DateTime? DateValue { get; set; }
    /// <summary>
    /// true if resolved, false if not resolved, null if resolved as ambiguous
    /// </summary>
    public bool? Resolved { get; set; }
    // the median value can be found at community_prediction.history.???.x1.q2
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dynamicQuestion">Dynamic Json object, that supports the dot notation for getting its elements.</param>
    /// <exception cref="Exception">Throws exception when the type of the question isn't supported.</exception>
    public AlertQuestion(dynamic dynamicQuestion)
    {
        Id = dynamicQuestion.id;
        Title = dynamicQuestion.title;
        string type = dynamicQuestion.possibilities.type;
        string? format = dynamicQuestion.possibilities.format; // todo: test that this does not throw unrecoverable exception

        double? resolution = dynamicQuestion.resolution;

        if (resolution is null)
        {
            Resolved = false;
            Value = dynamicQuestion.community_prediction.full.q2; // the current prediction
        }
        else if (Math.Abs((double) resolution - (-1)) < .000001) Resolved = null; 
        else
        {
            Resolved = true;
            Value = (double) resolution; // convert it to something usable
        }

        switch (type)
        {
            case "continuous":
                if (format == "date")
                {
                    Type = QuestionType.Date;
                    
                }
                else // format is numeric
                {
                    Type = QuestionType.Continuous;
                    double min = dynamicQuestion.possibilities.scale.min;
                    double max = dynamicQuestion.possibilities.scale.max;
                }
                break;
            case "binary":
                Type = QuestionType.Binary;
                break;
            default:
                throw new Exception("Unknown question type");

        }
        // todo populate properties with values to be used in printing

    }

    // recover date from 0-1 value
    private static DateTime ScaleDate(DateTime start, DateTime end, double prediction, double derivRatio)
    {
        if (Math.Abs(derivRatio - 1) < 0.000001)
            return start + (end - start) * prediction;
        
        return start + (end - start) * (Math.Pow(derivRatio , prediction) - 1) / (derivRatio - 1);
    } 
    // recover number from 0-1 value
    private static double ScaleNum(double start, double end, double prediction, double derivRatio)
    {
        if (Math.Abs(derivRatio - 1) < 0.000001)
            return start + (end - start) * prediction;
        
        return start + (end - start) * (Math.Pow(derivRatio , prediction) - 1) / (derivRatio - 1);
    }

    public bool IsSmallSwing()
    {
        throw new NotImplementedException();
    }

    public bool IsBigSwing()
    {
        throw new NotImplementedException();
    }
}