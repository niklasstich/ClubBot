using System.Data;
using ClubBotData;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClubBotLogic;

public class CountingHandler
{
    private ILogger<CountingHandler> _logger;
    private readonly IDbContextFactory<CountingDbContext> _dbContextFactory;

    public CountingHandler(ILogger<CountingHandler> logger, IDbContextFactory<CountingDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }
    public async Task HandleMessageAsync(SocketUserMessage message)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        //find registered channel
        var channel = await db.Channels.FirstOrDefaultAsync(ch => ch.GuildChannelId == message.Channel.Id);
        if (channel == null)
        {
            _logger.LogTrace($"got called in channel id {message.Channel} but no such channel is found" +
                             $"in the database");
            return;
        }

        if (message.Author is not SocketGuildUser user)
        {
            _logger.LogError("Can't cast message author to SocketGuildUser");
            return;
        }
        if (channel.BannedUsers.Any(bannedUser => bannedUser.UserId == user.Id))
        {
            await message.ReplyAsync("You're banned, loser.");
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
            //failure, reset count and check if new highscore
            await HandleIncorrectCountAsync(message, channel, parsedNumber, filteredMessage, invalidExpression);
        }

        await db.SaveChangesAsync();
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
        if (channel.BanActive)
        {
            if (message.Author is SocketGuildUser user)
            {
                channel.BannedUsers.Add(new BannedUser(channel.ChannelId, user.Id));
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