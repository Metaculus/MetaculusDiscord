using Discord.Commands;
using Discord.Interactions;

namespace MetaculusDiscord.Modules;

public interface IDataEnabled
{
    public Data.Data Data { get; set; }
}

public abstract class BotModuleBase : ModuleBase<SocketCommandContext>, IDataEnabled
{
    protected BotModuleBase(Data.Data data)
    {
        Data = data;
    }

    public Data.Data Data { get; set; }
}

public abstract class BotInteractionModuleBase : InteractionModuleBase, IDataEnabled
{
    protected BotInteractionModuleBase(Data.Data data)
    {
        Data = data;
    }

    public Data.Data Data { get; set; }
}