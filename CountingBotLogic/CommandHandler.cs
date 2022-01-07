using System.Reflection;
using System.Text.RegularExpressions;
using CountingBotData;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CountingBotLogic;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly CountingHandler _countingHandler;
    public CommandHandler(DiscordSocketClient client, CommandService commands, CountingHandler countingHandler)
    {
        _commands = commands;
        _client = client;
        _countingHandler = countingHandler;
    }

    public async Task InstallCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message) return;

        var argPos = 0;

        if (message.Author.IsBot) return;
        if (message.HasStringPrefix("~~", ref argPos))
        {
            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, null);
            return;
        }

        if (StartsWithNumber(message.Content) && await ChannelListened(message.Channel))
            await _countingHandler.HandleMessageAsync(message);
        if (message.Content=="test")
            await message.Channel.SendMessageAsync("peepoWtf");
    }

    private async Task<bool> ChannelListened(ISocketMessageChannel messageChannel)
    {
        using var db = new CountingDbContext();
        return await db.Channels.AnyAsync(ch => messageChannel.Id == ch.GuildChannelId);
    }

    private static bool StartsWithNumber(string message) => Regex.IsMatch(message, @"^\d+");
}