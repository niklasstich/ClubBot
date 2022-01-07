using System.Data;
using System.Text.RegularExpressions;
using CountingBotData;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CountingBotLogic;

public class CountingHandler
{
    private ILogger<CountingHandler> _logger;
    public CountingHandler(ILogger<CountingHandler> logger)
    {
        _logger = logger;
    }
    public async Task HandleMessageAsync(SocketUserMessage message)
    {
        await using (var db = new CountingDbContext())
        {
            //find registered channel
            var channel = await db.Channels.FirstOrDefaultAsync(ch => ch.GuildChannelId == message.Channel.Id);
            if (channel == null)
            {
                _logger.LogTrace($"got called in channel id {message.Channel} but no such channel is found" +
                                 $"in the database");
                return;
            }
            
            var automata = new MathExpressionAutomata(message.Content);
            var filteredMessage = automata.ParseExpression();
            var invalidExpression = automata.GetCurrentState() is MathExpressionAutomata.Terminate;
            var parsedNumber = Convert.ToInt32(new DataTable().Compute(filteredMessage, null));
            
            if (channel.Count.CurrentCount + 1 == parsedNumber)
            {
                await HandleCorrectCountAsync(message, channel);
            }
            else
            {
                //TODO: implement
                //failure, reset count and check if new highscore
                await HandleIncorrectCountAsync(message, channel, parsedNumber, filteredMessage, invalidExpression);
            }

            await db.SaveChangesAsync();
        }
    }

    private async Task HandleCorrectCountAsync(SocketUserMessage message, Channel channel)
    {
        var currentTime = DateTime.Now;
        var count = channel.Count;
        _logger.LogInformation($"Count success in channel {channel.ChannelId} guild {channel.GuildId} " +
                               $"newCount {count.CurrentCount+1}");
        UpdateCount(message, count, currentTime, count.CurrentCount+1);
        await message.AddReactionsAsync(new IEmote[]{new Emoji("\u2611️")});
    }

    private async Task HandleIncorrectCountAsync(SocketUserMessage message, Channel channel, int parsedNumber,
        string filteredMessage, bool invalidExpression)
    {
        var response = $"Expression `{message.Content}` evalulated to `{filteredMessage}` = {parsedNumber}, " +
                       $"but should have been {channel.Count.CurrentCount+1}. " +
                       $"Failed at {channel.Count.CurrentCount}! Next number is 1!";
        var currentTime = DateTime.Now;
        var count = channel.Count;
        _logger.LogInformation($"Count fail in channel {channel.ChannelId} guild {channel.GuildId} desiredCount " +
                               $"{count.CurrentCount+1} message {message.Content} filtered {filteredMessage} " +
                               $"evaluated {parsedNumber} invalidexpression {invalidExpression}");
        UpdateCount(message, count, currentTime, 0);
        if (channel.BanRoleActive && channel.BanRoleId != 0)
        {
            var addRoleTask = (message.Author as SocketGuildUser)?.AddRoleAsync(
                (message.Channel as SocketGuildChannel)?.Guild.GetRole(channel.BanRoleId));
            if (addRoleTask != null)
            {
                await addRoleTask;
                response += " Get banned loser lol";
            }
            else
            {
                _logger.LogWarning(
                    $"Failed to get addRoleTask for guildId {channel.GuildId} " +
                    $"guildChannelId {channel.GuildChannelId} roleId {channel.BanRoleId}");
            }
        }
        await message.AddReactionAsync(new Emoji("❌"));
        await message.ReplyAsync(response);
    }

    private static void UpdateCount(SocketUserMessage message, Count count, DateTime currentTime, int newCount)
    {
        count.CurrentCount = newCount;
        count.LastCountTime = currentTime;
        count.LastUserId = message.Author.Id;
        if (count.CurrentCount <= count.MaxCount) return;
        count.MaxCount = count.CurrentCount;
        count.MaxCountTime = currentTime;
        count.MaxUserId = message.Author.Id;
    }
}