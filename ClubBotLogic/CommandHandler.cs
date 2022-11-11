﻿using System.Reflection;
using System.Text.RegularExpressions;
using ClubBotData;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace ClubBotLogic;

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
        _client.MessageReceived += HandleCommandAsync;
        await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
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

        if (StartsWithNumber(message.Content) && await ChannelListened(message.Channel))
            await _countingHandler.HandleMessageAsync(message);
    }

    private async Task<bool> ChannelListened(ISocketMessageChannel messageChannel)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Channels.AnyAsync(ch => messageChannel.Id == ch.GuildChannelId);
    }

    private static bool StartsWithNumber(string message) => Regex.IsMatch(message, @"^\d+");
}