using Discord.Commands;
using Discord.Interactions;

namespace MetaculusDiscord.Modules;

public interface IDataInteractingModule
{
    public Data.Data Data { get; set; }
}

/// <summary>
/// Command module with injected Data access layer.
/// </summary>
public abstract class BotModuleBase : ModuleBase<SocketCommandContext>, IDataInteractingModule
{
    protected BotModuleBase(Data.Data data)
    {
        Data = data;
    }

    public Data.Data Data { get; set; }
}

/// <summary>
/// Interaction module (for slash commands) with injected Data.
/// </summary>
public abstract class BotInteractionModuleBase : InteractionModuleBase, IDataInteractingModule
{
    protected BotInteractionModuleBase(Data.Data data)
    {
        Data = data;
    }

    public Data.Data Data { get; set; }
}