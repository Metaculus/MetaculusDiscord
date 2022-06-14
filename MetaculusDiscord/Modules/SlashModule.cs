using Discord.Commands;
using Discord.Interactions;

namespace MetaculusDiscord.Modules;

public class SlashModule : BotModuleBase
{
    [Discord.Interactions.SlashCommand("metaculus","")]
    public async Task command()
    {
        Console.WriteLine("hi this is slash command");
    }
    
    public SlashModule(Data.Data data) : base(data)
    {
    }
}