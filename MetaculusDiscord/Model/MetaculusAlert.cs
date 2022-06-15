namespace MetaculusDiscord.Model;

public class MetaculusAlert
{
    public bool IsPrivate { get; set; }
    public ulong ChannelId { get; set; }
    public ulong UserId { get; set; }
    public int QuestionId { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastAlert { get; set; }


    public MetaculusAlert(ulong channelId, ulong userId, int questionId, DateTime created, DateTime lastAlert)
    {
        ChannelId = channelId;
        UserId = userId;
        QuestionId = questionId;
        Created = created;
        LastAlert = lastAlert;
    }
}