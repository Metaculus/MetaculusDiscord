using Discord.Commands;
using Discord.Interactions;
using MetaculusDiscord.Data;

namespace MetaculusDiscord.Modules;

public interface IDataEnabled
{
    public Data.Data Data { get; set; }
}

public abstract class BotModuleBase : ModuleBase<SocketCommandContext>, IDataEnabled
{
    public Data.Data Data { get; set; }

    protected BotModuleBase(Data.Data data)
    {
        Data = data;
    }
}

public abstract class BotInteractionModuleBase : InteractionModuleBase, IDataEnabled
{
    public Data.Data Data { get; set; }

    protected BotInteractionModuleBase(Data.Data data)
    {
        Data = data;
    }
}