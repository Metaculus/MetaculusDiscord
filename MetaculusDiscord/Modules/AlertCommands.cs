using Discord.Commands;
using MetaculusDiscord.Model;

namespace MetaculusDiscord.Modules;

public class AlertCommands : BotModuleBase
{
    [Command("setalert")]
    public async Task SetAlert(int question)
    {
        MetaculusAlert alert =
            new MetaculusAlert(Context.Channel.Id, Context.User.Id, question, DateTime.Now, DateTime.Now);
        Console.WriteLine("hello this is an alert");
    }
    
    
    public AlertCommands(Data.Data data) : base(data)
    {
    }
}