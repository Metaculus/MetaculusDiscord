using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetaculusDiscord.Model;

public abstract class QuestionAlert //: IIdentifiable<ulong>
{
    [Key]
    [Column("alert_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("question_id")] public ulong QuestionId { get; set; }
}

public class UserQuestionAlert : QuestionAlert
{
    [Column("user_id")] public ulong UserId { get; set; }
}

public class ChannelQuestionAlert : QuestionAlert
{
    [Column("channel_id")] public ulong ChannelId { get; set; }
}