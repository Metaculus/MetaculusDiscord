using Discord.Commands;

namespace MetaculusDiscord.Modules;

public class UtilCommands : BotModuleBase
{
    public UtilCommands(Data.Data data) : base(data)
    {
    }

    [Command("help")]
    public async Task Help()
    {
        await Context.Channel.SendMessageAsync(
            "The bot currently supports the following commands: \n   `!mc search <query>`, `search` can be replaced by `s`" +
            "\n" +
            "to select from the results, press the reaction with the corresponding number");
    }
}