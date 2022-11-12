using System.Data;
using ClubBot.Data.Common;
using ClubBot.Data.Counting;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClubBot.Logic.Counting;

public class CountingHandler
{
    private ILogger<CountingHandler> _logger;
    private readonly IDbContextFactory<CountingDbContext> _countDbContextFactory;

    public CountingHandler(ILogger<CountingHandler> logger, IDbContextFactory<CountingDbContext> countDbContextFactory)
    {
        _logger = logger;
        _countDbContextFactory = countDbContextFactory;
    }
    public async Task HandleCountMessageAsync(SocketUserMessage message)
    {
        await using var db = await _countDbContextFactory.CreateDbContextAsync();
        //find registered channel
        var channel = await db.Channels.FirstOrDefaultAsync(ch => ch.GuildChannelId == message.Channel.Id);
        if (channel == null)
        {
            _logger.LogTrace($"got called in channel id {message.Channel} but no such channel is found" +
                             $"in the database");
            return;
        }

        var countSettings = await db.FindCountSettingsOrCreateNewAsync(message.Channel.Id);

        if (message.Author is not SocketGuildUser user)
        {
            _logger.LogError("Can't cast message author to SocketGuildUser");
            return;
        }
        if (db.BannedUsers.Any(bu => bu.UserId == user.Id))
        {
            await message.ReplyAsync("You're banned, loser.");
            return;
        }
            
        var automata = new MathExpressionAutomata(message.Content);
        var filteredMessage = automata.ParseExpression();
        var invalidExpression = automata.GetCurrentState() is MathExpressionAutomata.Terminate;
        var parsedNumber = Convert.ToInt32(new DataTable().Compute(filteredMessage, null));

        var count = await db.Counts.FirstOrDefaultAsync(count => count.CountSettings.Channel == channel);
        if (count is null)
        {
            _logger.LogTrace("could not find count object for channel id {}", message.Channel.Id);
            return;
        }
        if (count.CurrentCount + 1 == parsedNumber)
        {
            await HandleCorrectCountAsync(message, channel, count);
        }
        else
        {
            //failure, reset count and check if new highscore
            var settings = await db.CountSettings.FirstOrDefaultAsync(countSettings => countSettings.Channel == channel);
            if (settings is null)
            {
                _logger.LogTrace("could not find countsettings object for channel id {}", message.Channel.Id);
                return;
            }
            await HandleIncorrectCountAsync(message, channel, count, settings, parsedNumber, filteredMessage, invalidExpression);
        }

        await db.SaveChangesAsync();
    }

    private async Task HandleCorrectCountAsync(SocketUserMessage message, Channel channel, Count count)
    {
        var currentTime = DateTime.Now;
        _logger.LogInformation($"Count success in channel {channel.ChannelId} guild {channel.GuildId} " +
                               $"newCount {count.CurrentCount+1}");
        UpdateCount(message, count, currentTime, count.CurrentCount+1);
        await message.AddReactionsAsync(new IEmote[]{new Emoji("\u2611️")});
    }

    private async Task HandleIncorrectCountAsync(SocketUserMessage message, Channel channel, Count count, CountSettings settings,
        int parsedNumber, string filteredMessage, bool invalidExpression)
    {
        var response = $"Expression `{message.Content}` evalulated to `{filteredMessage}` = {parsedNumber}, " +
                       $"but should have been {count.CurrentCount+1}. " +
                       $"Failed at {count.CurrentCount}! Next number is 1!";
        var currentTime = DateTime.Now;
        _logger.LogInformation($"Count fail in channel {channel.ChannelId} guild {channel.GuildId} desiredCount " +
                               $"{count.CurrentCount+1} message {message.Content} filtered {filteredMessage} " +
                               $"evaluated {parsedNumber} invalidexpression {invalidExpression}");
        UpdateCount(message, count, currentTime, 0);
        if (settings.BanActive)
        {
            if (message.Author is SocketGuildUser user)
            {
                var db = await _countDbContextFactory.CreateDbContextAsync();
                db.BannedUsers.Add(new BannedUser(user.Id));
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    
                }
            }
            response += " Get banned loser lol";
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