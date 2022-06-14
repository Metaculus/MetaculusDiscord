using Discord.Commands;
using MetaculusDiscord.Data;

namespace MetaculusDiscord.Modules;

public class UtilCommands: BotModuleBase
{
    [Command("help")]
    public async Task Help()
    {
        await Context.Channel.SendMessageAsync("The bot supports `search <query>`");
    }

    public UtilCommands(Data.Data data) : base(data)
    {
    }
}