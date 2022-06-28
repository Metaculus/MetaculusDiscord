using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetaculusDiscord.Model;

/// <summary>
///     Defines base of an alert that can be put into the database.
/// </summary>
public abstract class Alert
{
    [Key]
    [Column("alert_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}

/// <summary>
/// Alerts that are for a single question.
/// </summary>
public abstract class QuestionAlert : Alert
{
    [Column("question_id")] public long QuestionId { get; set; }
}

/// <summary>
///    Alert for a single question to user DM.
/// </summary>
public sealed class UserQuestionAlert : QuestionAlert
{
    [Column("user_id")] public ulong UserId { get; set; }
}

/// <summary>
/// Alert for a single question to a server channel.
/// </summary>
public sealed class ChannelQuestionAlert : QuestionAlert
{
    [Column("channel_id")] public ulong ChannelId { get; set; }
}

/// <summary>
/// Alert for a category of questions.
/// </summary>
public class CategoryAlert : Alert
{
    [Column("category_id")] public string CategoryId { get; set; } = "";
}

/// <summary>
/// Alert for a category of questions for a public channel.
/// </summary>
public sealed class ChannelCategoryAlert : CategoryAlert
{
    [Column("channel_id")] public ulong ChannelId { get; set; }
}