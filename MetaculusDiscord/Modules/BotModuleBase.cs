using Discord.Commands;
using MetaculusDiscord.Data;
namespace MetaculusDiscord.Modules;

public abstract class BotModuleBase : ModuleBase<SocketCommandContext>
{
    public static readonly HttpClient HttpClient = new HttpClient();
    public Data.Data Data { get; set; }

    protected BotModuleBase(Data.Data data)
    {
        Data = data;
    }
}