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
    [RequireOwner(Group = "permission")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "permission")]
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
    [RequireOwner(Group = "permission")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "permission")]
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
    [RequireOwner(Group = "permission")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "permission")]
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

    [Command("registerbanrole")]
    [Summary("Register a role as ban role")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "permission")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "permission")]
    public async Task RegisterBanrole(
        [Summary("The role that should be given when the count is failed.")] IRole banRole)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.Channels.FirstOrDefaultAsync(ch => Context.Channel.Id == ch.GuildChannelId);
        if (channel == null)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        channel.BanRoleId = banRole.Id;
        await db.SaveChangesAsync();
        await ReplyAsync($"Set role {banRole.Name} as banrole");
    }

    [Command("toggleban")]
    [Summary("Toggles ban on fail.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "permission")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "permission")]
    public async Task RegisterBanrole(
        [Summary("Should toggleban be True or False?")] bool toggle)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.Channels.FirstOrDefaultAsync(ch => Context.Channel.Id == ch.GuildChannelId);
        if (channel == null)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        if (channel.BanRoleId == 0)
        {
            await ReplyAsync("Please set a banrole with `~~registerbanrole` first");
            return;
        }

        channel.BanRoleActive = toggle;
        await db.SaveChangesAsync();
        await ReplyAsync($"Set ban on fail to {toggle}");
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