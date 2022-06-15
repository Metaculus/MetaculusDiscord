using Discord.Commands;
using MetaculusDiscord.Data;

namespace MetaculusDiscord.Modules;

public class UtilCommands : BotModuleBase
{
    [Command("help")]
    public async Task Help()
    {
        await Context.Channel.SendMessageAsync(
            "The bot currently supports the following commands: \n   `!mc search <query>`, `search` can be replaced by `s`" +
            "\n" +
            "to select from the results, press the reaction with the corresponding number");
    }

    public UtilCommands(Data.Data data) : base(data)
    {
    }
}