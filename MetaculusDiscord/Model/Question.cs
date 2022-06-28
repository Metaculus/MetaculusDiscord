using System.Globalization;
using Microsoft.CSharp.RuntimeBinder;

namespace MetaculusDiscord.Model;

/// <summary>
///     Representation of a Metaculus question.
/// <remarks>Note that it has no constructor, because it's not possible to chain constructors with *dynamic* parameter.
/// This leads to that all the children have to set the common properties by themselves.</remarks>
/// </summary>
public abstract class Question
{
    /// <summary>
    /// Supported question types.
    /// </summary>
    public enum QuestionType
    {
        Continuous,
        Binary,
        Date
    }

    public long Id { get; protected init; }
    public string Title { get; protected init; } = "";
    public QuestionType Type { get; protected init; }
    public DateTime PublishTime { get; protected init; }

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
        PublishTime = DateTime.Parse((string) dynamicQuestion.publish_time);
        PageUrl = dynamicQuestion.page_url;
    }

    /// <summary>
    ///     Long page url so that the user can see the question title in it
    /// </summary>
    public string PageUrl { get; }
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
    /// <remarks>Because the API is not consistent, there is some edge case handling and try blocks to avoid an error.</remarks>
    /// <param name="dynamicQuestion">Dynamic Json object, that supports the dot notation for getting its elements.</param>
    /// <exception cref="Exception">Throws exception when the type of the question isn't supported.</exception>
    public AlertQuestion(dynamic dynamicQuestion)
    {
        Id = dynamicQuestion.id;
        Title = dynamicQuestion.title;
        PublishTime = DateTime.Parse((string) dynamicQuestion.publish_time);

        double? resolution = dynamicQuestion.resolution;
        _rawSixHoursOldValue =
            PredictionWithClosestTime(dynamicQuestion.community_prediction.history, DateTime.Now.AddHours(-6));
        _rawDayOldValue =
            PredictionWithClosestTime(dynamicQuestion.community_prediction.history, DateTime.Now.AddDays(-1));
        if (resolution is null)
        {
            Resolved = false;
            try
            {
                _rawValue = dynamicQuestion.community_prediction.full.q2; // the current prediction 
            } // sometimes this field is not in the API
            catch (RuntimeBinderException)
            {
                _rawValue = 0;
            }
        }
        else if (Math.Abs((double) resolution - -1) < .000001) // -1 denotes ambiguous
        {
            Resolved = null;
            _rawValue = dynamicQuestion.community_prediction.full.q2; // the current prediction
        }
        else
        {
            Resolved = true;
            _rawValue = (double) resolution;
        }

        string type = dynamicQuestion.possibilities.type;
        string? format = dynamicQuestion.possibilities.format;
        switch (type)
        {
            case "binary":
                Type = QuestionType.Binary;
                Value = _rawValue;
                DayOldValue = _rawDayOldValue;
                SixHoursOldValue = _rawSixHoursOldValue;
                break;

            case "continuous":

                double derivRatio = dynamicQuestion.possibilities.scale.deriv_ratio;
                if (format == "date")
                {
                    string min = dynamicQuestion.possibilities.scale.min;
                    string max = dynamicQuestion.possibilities.scale.max;
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
                    double start = dynamicQuestion.possibilities.scale.min;
                    double end = dynamicQuestion.possibilities.scale.max;
                    Type = QuestionType.Continuous;
                    Value = ScaleNum(start, end, _rawValue, derivRatio);
                    DayOldValue = ScaleNum(start, end, _rawDayOldValue, derivRatio);
                    SixHoursOldValue = ScaleNum(start, end, _rawSixHoursOldValue, derivRatio);
                }

                break;
            default:
                throw new Exception("Unknown question type");
        }
    }

    /// <summary>
    /// For binary questions the median forecast and for continuous questions the value of the median.
    /// </summary>
    public double Value { get; }

    public double DayOldValue { get; }
    public double SixHoursOldValue { get; }

    /// <summary>
    /// Date questions only. The value of the median forecast.
    /// </summary>
    public DateTime? DateValue { get; }

    public DateTime? DayOldDateValue { get; }
    public DateTime? SixHoursOldDateValue { get; }

    /// <summary>
    ///     true if resolved, false if not resolved, null if resolved as ambiguous
    /// </summary>
    public bool? Resolved { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="history">Dynamic object that holds the community prediction history.</param>
    /// <param name="time">Time that we want to minimize distance to.</param>
    /// <returns></returns>
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

    /// <summary>
    /// The prediction is in in the range 0-1 and it's necessary to scale it to the range of the question.
    /// </summary>
    /// <remarks> *+- operators defined on DateTime do the hard work here.</remarks>
    /// <param name="start">Start date of the forecast options.</param>
    /// <param name="end">End date of the forecast options</param>
    /// <param name="prediction">The prediction to be scale</param>
    /// <param name="derivRatio"></param>
    /// <returns>The scaled date.</returns>
    private static DateTime ScaleDate(DateTime start, DateTime end, double prediction, double derivRatio)
    {
        if (Math.Abs(derivRatio - 1) < 0.000001)
            return start + (end - start) * prediction;

        return start + (end - start) * (Math.Pow(derivRatio, prediction) - 1) / (derivRatio - 1);
    }

    /// <summary>
    /// Same as above but for continuous questions.
    /// </summary>
    /// <returns>The scaled number.</returns>
    private static double ScaleNum(double start, double end, double prediction, double derivRatio)
    {
        if (Math.Abs(derivRatio - 1) < 0.000001)
            return start + (end - start) * prediction;

        return start + (end - start) * (Math.Pow(derivRatio, prediction) - 1) / (derivRatio - 1);
    }

    /// <returns>Whether the question changed more than the threshold in the last 6 hours. </returns>
    public bool Is6HourSwing(double threshold)
    {
        if (_rawValue == 0) return false; // the 0 prediction would mean that no one has answered the question yet.
        if (Math.Abs(SixHourSwing()) > threshold)
            return true;
        return false;
    }

    /// <returns>Whether the question changed more than the threshold in the last day. </returns>
    public bool IsDaySwing(double threshold)
    {
        if (_rawValue == 0) return false; // the 0 prediction would mean that no one has answered the question yet.
        if (Math.Abs(DaySwing()) > threshold)
            return true;
        return false;
    }

    /// <returns>Daily percent change in the median prediction</returns>
    public double DaySwing()
    {
        return _rawValue - _rawDayOldValue;
    }

    /// <returns>Six hour percent change in the median prediction</returns>
    public double SixHourSwing()
    {
        return _rawValue - _rawSixHoursOldValue;
    }

    /// <returns>String representation of the value.</returns>
    public string ValueString()
    {
        return ValueString(Value, DateValue);
    }

    /// <returns>String representation of the day old value.</returns>
    public string DayOldValueString()
    {
        return ValueString(DayOldValue, DayOldDateValue);
    }


    /// <returns>String representation of the six hour old value.</returns>
    public string SixHoursOldValueString()
    {
        return ValueString(SixHoursOldValue, SixHoursOldDateValue);
    }

    /// <summary>
    /// Creates string representation depending on the question type.
    /// </summary>
    /// <param name="number">The value used for binary and numeric continuous questions.</param>
    /// <param name="date">The value used for date questions</param>
    /// <returns>String representation</returns>
    private string ValueString(double number, DateTime? date)
    {
        return Type switch
        {
            QuestionType.Binary when number > 0.995 => "Yes",
            QuestionType.Binary when number < 0.005 => "No",
            QuestionType.Binary => Math.Round(number * 100, 2) + "%",
            QuestionType.Continuous => number.ToString(CultureInfo.CurrentCulture),
            QuestionType.Date => date!.Value.ToString("yyyy-MM-dd"),
            _ => throw new Exception("Unknown question type")
        };
    }
}