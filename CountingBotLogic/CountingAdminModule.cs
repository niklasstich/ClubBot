using CountingBotData;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CountingBotLogic;

public class CountingAdminModule : ModuleBase<SocketCommandContext>
{
    private readonly IDbContextFactory<CountingDbContext> _dbContextFactory;
    private readonly ILogger _logger;

    public CountingAdminModule(IDbContextFactory<CountingDbContext> dbContextFactory, ILogger<CountingAdminModule> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
    
    [Command("register")]
    [Summary("Registers the channel for listening.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task RegisterAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        if (await db.Channels.AnyAsync(ch => Context.Channel.Id == ch.GuildChannelId))
        {
            await ReplyAsync("This channel is already registered!");
            return;
        }

        var count = new Count();
        var channel = new Channel
            { GuildId = Context.Guild.Id, GuildChannelId = Context.Channel.Id, Count = count};
        count.Channel = channel;
        db.Counts.Add(count);
        db.Channels.Add(channel);
        await db.SaveChangesAsync();

        await ReplyAsync("Channel registered for listening.");
    }

    [Command("unregister")]
    [Summary("Unregisteres the channel for listening.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task UnregisterAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.Channels.FirstOrDefaultAsync(ch => Context.Channel.Id == ch.GuildChannelId);
        if (channel == null)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        db.Counts.Remove(channel.Count);
        db.Channels.Remove(channel);
        await db.SaveChangesAsync();
        await ReplyAsync("Channel unregistered for listening");
    }

    [Command("restore")]
    [Summary("Restore a count in a channel.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task RestoreAsync([Summary("The count that is to be restored.")] int count)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.Channels.FirstOrDefaultAsync(ch => Context.Channel.Id == ch.GuildChannelId);
        if (channel == null)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        channel.Count.CurrentCount = count;
        await db.SaveChangesAsync();
        await ReplyAsync($"Restored count to {count}, next number is {count + 1}!");
    }

    [Command("toggleban")]
    [Summary("Toggles ban on fail.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task ToggleBan(
        [Summary("Should toggleban be True or False?")] bool toggle)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.Channels.FirstOrDefaultAsync(ch => Context.Channel.Id == ch.GuildChannelId);
        if (channel == null)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        channel.BanActive = toggle;
        await db.SaveChangesAsync();
        await ReplyAsync($"Set ban on fail to {toggle}");
    }

    [Command("ban")]
    [Summary("Bans a user from counting in the channel.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task Ban(
        [Summary("The user that should be banned.")]
        SocketGuildUser user)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.Channels.FirstOrDefaultAsync(ch => Context.Channel.Id == ch.GuildChannelId);
        if (channel == null)
        {
            await ReplyAsync("Failed to get channel from db");
            return;
        }

        if (!channel.BannedUsers.Any(bannedUser =>
                bannedUser.UserId == user.Id && bannedUser.ChannelId == channel.ChannelId))
        {
            channel.BannedUsers.Add(new BannedUser(channel.ChannelId, user.Id));
        }

        try
        {
            await db.SaveChangesAsync();
            await ReplyAsync($"Banned user {user.DisplayName} from counting.");
        }
        catch (DbUpdateException dbex)
        {
            await ReplyAsync("Exception while updating DB: " + dbex.Message);
        }
    }
    
    [Command("unban")]
    [Summary("Unbans a user from counting in the channel.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task Unban(
        [Summary("The user that should be unbanned.")]
        SocketGuildUser user)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.Channels.FirstOrDefaultAsync(ch => Context.Channel.Id == ch.GuildChannelId);
        if (channel == null)
        {
            await ReplyAsync("Failed to get channel from db");
            return;
        }

        var banCount = channel.BannedUsers.RemoveAll(bannedUser =>
            bannedUser.UserId == user.Id && bannedUser.ChannelId == channel.ChannelId);
        if (banCount != 1)
        {
            await ReplyAsync($"Ban deletion yielded {banCount} results, but expected 1");
        }

        try
        {
            await db.SaveChangesAsync();
            await ReplyAsync($"Unbanned user {user.DisplayName} from counting.");
        }
        catch (DbUpdateException dbex)
        {
            await ReplyAsync("Exception while updating DB: " + dbex.Message);
        }
    }
    

    [Command("currentcount")]
    [Summary("Displays the current count in a channel.")]
    [RequireContext(ContextType.Guild)]
    public async Task CurrentCountAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.Channels.FirstOrDefaultAsync(ch => Context.Channel.Id == ch.GuildChannelId);
        if (channel == null)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        await ReplyAsync(
            $"Current count is {channel.Count.CurrentCount}, next number is {channel.Count.CurrentCount + 1}");
    }
    
    //TODO: command to ban users, only bot owner can execute

}