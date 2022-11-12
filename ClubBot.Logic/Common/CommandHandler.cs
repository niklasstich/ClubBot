using System.Reflection;
using System.Text.RegularExpressions;
using ClubBot.Data.Counting;
using ClubBot.Logic.Counting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace ClubBot.Logic.Common;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly CountingHandler _countingHandler;
    private readonly IServiceProvider _services;
    private readonly IDbContextFactory<CountingDbContext> _dbContextFactory;

    public CommandHandler(DiscordSocketClient client, CommandService commandService, CountingHandler countingHandler, 
        IServiceProvider services, IDbContextFactory<CountingDbContext> dbContextFactory)
    {
        _commandService = commandService;
        _client = client;
        _countingHandler = countingHandler;
        _services = services;
        _dbContextFactory = dbContextFactory;
    }

    public async Task InstallCommandsAsync()
    {
        _client.MessageReceived += HandleMessageAsync;
        await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
    }

    private async Task HandleMessageAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message) return;

        var argPos = 0;

        if (message.Author.IsBot) return;
        if (message.HasStringPrefix("~~", ref argPos))
        {
            var context = new SocketCommandContext(_client, message);
            var result = await _commandService.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
            {
                await message.ReplyAsync(result.ErrorReason);
            }
            return;
        }

        if (StartsWithNumber(message.Content) && await ChannelListenedAsync(message.Channel))
            await _countingHandler.HandleCountMessageAsync(message);
    }

    private async Task<bool> ChannelListenedAsync(ISocketMessageChannel messageChannel)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.CountSettings.AnyAsync(cs => cs.Channel.GuildChannelId == messageChannel.Id && cs.CountingActive);
    }

    private static bool StartsWithNumber(string message) => Regex.IsMatch(message, @"^\d+");
}