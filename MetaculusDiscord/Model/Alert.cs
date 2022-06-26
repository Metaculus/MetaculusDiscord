using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetaculusDiscord.Model;

/// <summary>
///     Defines base of an alert that to be put into the database.
/// </summary>
public abstract class Alert 
{
    [Key]
    [Column("alert_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}

public abstract class QuestionAlert : Alert
{
    [Column("question_id")] public long QuestionId { get; set; }
}

/// <summary>
///     Question with an id of the user to be alerted.
/// </summary>
public class UserQuestionAlert : QuestionAlert
{
    [Column("user_id")] public ulong UserId { get; set; }
}

/// <summary>
///     Question with an id of the channel to be alerted.
/// </summary>
public class ChannelQuestionAlert : QuestionAlert
{
    [Column("channel_id")] public ulong ChannelId { get; set; }
}

public class CategoryAlert : Alert
{
    [Column("category_id")] public string CategoryId { get; set; }
}

public class ChannelCategoryAlert : CategoryAlert
{
    [Column("channel_id")] public ulong ChannelId { get; set; }
}