using System.Globalization;

namespace MetaculusDiscord.Model;

/// <summary>
///     Representation of a Metaculus question. Note that it has no constructor, because it's not possible to chain
///     constructors from *dynamic*.
/// </summary>
public abstract class Question
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

    public string ShortUrl()
    {
        return $"https://www.metaculus.com/questions/{Id}";
    }
}

/// <summary>
///     Question representation used for showing the user search results.
/// </summary>
public class SearchResultQuestion : Question
{
    public SearchResultQuestion(dynamic dynamicQuestion)
    {
        Id = dynamicQuestion.id;
        Title = dynamicQuestion.title;
        string publishTime = dynamicQuestion.publish_time;
        PublishTime = DateTime.Parse(publishTime).Date;
        PageUrl = dynamicQuestion.page_url;
    }

    /// <summary>
    ///     Long page url so that the user can see the question title in it
    /// </summary>
    public string PageUrl { get; set; }

    public DateTime PublishTime { get; set; }
}

/// <summary>
///     Question representation used for sending alerts.
/// </summary>
public class AlertQuestion : Question
{
    private readonly double _rawDayOldValue;
    private readonly double _rawSixHoursOldValue;
    private readonly double _rawValue;

    /// <summary>
    ///     Parses the question with the predictions from a dynamic json object.
    /// </summary>
    /// <param name="dynamicQuestion">Dynamic Json object, that supports the dot notation for getting its elements.</param>
    /// <exception cref="Exception">Throws exception when the type of the question isn't supported.</exception>
    public AlertQuestion(dynamic dynamicQuestion)
    {
        Id = dynamicQuestion.id;
        Title = dynamicQuestion.title;

        double? resolution = dynamicQuestion.resolution;
        _rawSixHoursOldValue =
            PredictionWithClosestTime(dynamicQuestion.community_prediction.history, DateTime.Now.AddHours(-6));
        _rawDayOldValue =
            PredictionWithClosestTime(dynamicQuestion.community_prediction.history, DateTime.Now.AddDays(-1));
        if (resolution is null)
        {
            Resolved = false;
            _rawValue = dynamicQuestion.community_prediction.full.q2; // the current prediction
        }
        else if (Math.Abs((double) resolution - -1) < .000001) // -1 denotes ambiguous
        {
            Resolved = null;
            _rawValue = dynamicQuestion.community_prediction.full.q2; // the current prediction
        }
        else
        {
            Resolved = true;
            _rawValue = (double) resolution; // convert it to something usable
        }

        string type = dynamicQuestion.possibilities.type;
        string?
            format = dynamicQuestion.possibilities
                .format; // todo: test that this does not throw unrecoverable exception
        switch (type)
        {
            case "binary":
                Type = QuestionType.Binary;
                Value = _rawValue;
                DayOldValue = _rawDayOldValue;
                SixHoursOldValue = _rawSixHoursOldValue;
                break;

            case "continuous":

                string min = dynamicQuestion.possibilities.scale.min;
                string max = dynamicQuestion.possibilities.scale.max;
                double derivRatio = dynamicQuestion.possibilities.scale.deriv_ratio;
                if (format == "date")
                {
                    Type = QuestionType.Date;
                    // string with date in format "yyyy-MM-dd"
                    var start = DateTime.Parse(min);
                    var end = DateTime.Parse(max);
                    Value = _rawValue;
                    DayOldValue = _rawDayOldValue;
                    SixHoursOldValue = _rawSixHoursOldValue;
                    DateValue = ScaleDate(start, end, _rawValue, derivRatio);
                    DayOldDateValue = ScaleDate(start, end, _rawDayOldValue, derivRatio);
                    SixHoursOldDateValue = ScaleDate(start, end, _rawSixHoursOldValue, derivRatio);
                }
                else // format is numeric
                {
                    Type = QuestionType.Continuous;
                    var start = double.Parse(min);
                    var end = double.Parse(max);
                    Value = ScaleNum(start, end, _rawValue, derivRatio);
                    DayOldValue = ScaleNum(start, end, _rawDayOldValue, derivRatio);
                    SixHoursOldValue = ScaleNum(start, end, _rawSixHoursOldValue, derivRatio);
                }

                break;
            default:
                throw new Exception("Unknown question type");
        }
    }

    public double Value { get; }
    public double DayOldValue { get; }
    public double SixHoursOldValue { get; }
    public DateTime? DateValue { get; }
    public DateTime? DayOldDateValue { get; }
    public DateTime? SixHoursOldDateValue { get; }

    /// <summary>
    ///     true if resolved, false if not resolved, null if resolved as ambiguous
    /// </summary>
    public bool? Resolved { get; set; }

    private double PredictionWithClosestTime(dynamic history, DateTime time)
    {
        // convert time to unix epoch
        var timeUnix = (long) time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        for (int i = history.Count - 1; i >= 0; i--)
            if (history[i].t < timeUnix)
                // return the median
                return history[i].x1.q2;
        return 0;
    }

    // recover date from 0-1 value
    private static DateTime ScaleDate(DateTime start, DateTime end, double prediction, double derivRatio)
    {
        if (Math.Abs(derivRatio - 1) < 0.000001)
            return start + (end - start) * prediction;

        return start + (end - start) * (Math.Pow(derivRatio, prediction) - 1) / (derivRatio - 1);
    }

    // recover number from 0-1 value
    private static double ScaleNum(double start, double end, double prediction, double derivRatio)
    {
        if (Math.Abs(derivRatio - 1) < 0.000001)
            return start + (end - start) * prediction;

        return start + (end - start) * (Math.Pow(derivRatio, prediction) - 1) / (derivRatio - 1);
    }

    public bool Is6HourSwing(double threshold)
    {
        if (Math.Abs(SixHourSwing()) > threshold)
            return true;
        return false;
    }

    public bool IsDaySwing(double threshold)
    {
        if (Math.Abs(DaySwing()) > threshold)
            return true;
        return false;
    }

    /// <returns>Daily percent change</returns>
    public double DaySwing()
    {
        return _rawValue - _rawDayOldValue;
    }

    /// <returns>Six hour percent change</returns>
    public double SixHourSwing()
    {
        return _rawValue - _rawSixHoursOldValue;
    }

    public string ValueString()
    {
        return ValueString(Value, DateValue);
    }

    public string DayOldValueString()
    {
        return ValueString(DayOldValue, DayOldDateValue);
    }

    public string SixHoursOldValueString()
    {
        return ValueString(SixHoursOldValue, SixHoursOldDateValue);
    }

    private string ValueString(double number, DateTime? time)
    {
        if (Type == QuestionType.Binary)
            if (number > 0.995)
                return "Yes"; 
            else if (number < 0.005)
                return "No";
            else return Math.Round(number * 100, 2) + "%";
        if (Type == QuestionType.Continuous)
            return number.ToString(CultureInfo.CurrentCulture);
        if (Type == QuestionType.Date)
            return time!.Value.ToString("yyyy-MM-dd");
        throw new Exception("Unknown question type");
    }
}