using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using MetaculusDiscord.Data;
using MetaculusDiscord.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MetaculusDiscord;

public class Program
{
    public static Task Main(string[] args)
    {
        return MainAsync();
    }

    /// <summary>
    ///     Configures and hosts the bots services.
    /// </summary>
    private static async Task MainAsync()
    {
        var builder = new HostBuilder().ConfigureAppConfiguration(a =>
            {
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .AddEnvironmentVariables()
                    .Build();
                a.AddConfiguration(config);
                /// var viewConfig = config.AsEnumerable().ToList();
                /// viewConfig.ForEach(p => Console.WriteLine(p));
            }).ConfigureLogging(x =>
            {
                x.AddConsole();
                x.SetMinimumLevel(LogLevel.Debug);
            }).ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig
                    {LogLevel = LogSeverity.Debug, AlwaysDownloadUsers = false, MessageCacheSize = 200};
                config.Token = context.Configuration["DiscordToken"];
            }).UseCommandService((_, config) =>
            {
                config.CaseSensitiveCommands = false;
                config.LogLevel = LogSeverity.Debug;
                config.DefaultRunMode = RunMode.Async;
            }).UseInteractionService((_, config) => { config.LogLevel = LogSeverity.Debug; }
            )
            .ConfigureServices((context, services) =>
            {
                services.AddDbContextFactory<MetaculusContext>(x =>
                    x.UseNpgsql(context.Configuration.GetConnectionString("Default")));
                services.AddHostedService<InteractionHandler>()
                    .AddSingleton<Data.Data>(); // injecting class Data into commandHandler
                services.AddHostedService<QuestionAlertService>()
                    .AddSingleton<Data.Data>(); // injecting class Data into alertService
                services.AddHostedService<CategoricalService>()
                    .AddSingleton<Data.Data>();
            })
            .UseConsoleLifetime();
        var host = builder.Build();
        using (host)
        {
            await host.RunAsync();
        }
    }
}