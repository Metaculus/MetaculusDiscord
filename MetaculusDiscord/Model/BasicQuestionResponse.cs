namespace MetaculusDiscord.Model;

public abstract class BasicQuestionResponse : IIdentifiable
{
    public ulong Id { get; set; }
}

public interface IIdentifiable
{
    public ulong Id { get; set; }
}