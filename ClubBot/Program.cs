// See https://aka.ms/new-console-template for more information

using ClubBot.Data.Counting;
using ClubBot.Logic.Common;
using ClubBot.Logic.Counting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClubBot;

internal static class Program
{
    static Task Main(string[] args) =>
        CreateHostBuilder(args).Build().RunAsync();

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
                services
                    .AddDbContextFactory<CountingDbContext>()
                    .AddTransient<CountingDbInitializer>()
                    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                    {
                        #if DEBUG
                        LogLevel = LogSeverity.Debug,
                        #else 
                        LogLevel = LogSeverity.Info,
                        #endif
                        GatewayIntents = GatewayIntents.MessageContent | (GatewayIntents.AllUnprivileged & ~GatewayIntents.GuildScheduledEvents & ~GatewayIntents.GuildInvites),
                        
                    }))
                    .AddSingleton<CommandService>()
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<CountingHandler>()
                    .AddLogging(builder =>
                    {
                        builder.ClearProviders();
                        builder.SetMinimumLevel(LogLevel.Trace);
                        builder.AddConsole();
                    })
                    .AddHostedService<DiscordWorker>());
}