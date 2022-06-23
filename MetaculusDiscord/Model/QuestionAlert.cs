using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetaculusDiscord.Model;
/// <summary>
/// Defines base of a question alert that to be put into the database.
/// </summary>
public abstract class QuestionAlert 
{
  
    [Key]
    [Column("alert_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("question_id")] public long QuestionId { get; set; }
}
/// <summary>
/// Question with an id of the user to be alerted.
/// </summary>
public class UserQuestionAlert : QuestionAlert
{
    [Column("user_id")] public ulong UserId { get; set; }
}
/// <summary>
/// Question with an id of the channel to be alerted.
/// </summary>
public class ChannelQuestionAlert : QuestionAlert
{
    [Column("channel_id")] public ulong ChannelId { get; set; }
}