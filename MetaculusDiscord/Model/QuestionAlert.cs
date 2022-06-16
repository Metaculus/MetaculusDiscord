namespace MetaculusDiscord.Model;
public delegate bool Resolution<in T,in Q>(T t, Q q);
// todo completely redesign this
public abstract class QuestionAlert
{
    public ulong AlertId { get; set; }
    public ulong QuestionId { get; set; }
    public Resolution<QuestionAlert,Question> Updated;
    public Resolution<QuestionAlert,Question> Resolved;
    public abstract bool Update(Question q);
    public abstract bool Resolve(Question q);
}

public abstract class PersonalQuestionAlert : QuestionAlert
{
    public ulong UserId { get; set; }
}

public abstract class ChannelQuestionAlert : QuestionAlert
{
    public ulong ChannelId { get; set; }
}