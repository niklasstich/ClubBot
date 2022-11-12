using ClubBot.Data.Counting;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClubBot.Logic.Counting;

public class CountingAdminModule : ModuleBase<SocketCommandContext>
{
    private readonly IDbContextFactory<CountingDbContext> _dbContextFactory;
    private readonly ILogger<CountingAdminModule> _logger;

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
        await SetCountActive(true);
        await ReplyAsync("Channel registered for listening.");
    }

    [Command("unregister")]
    [Summary("Unregisteres the channel for listening.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task UnregisterAsync()
    {
        await SetCountActive(false);
        await ReplyAsync("Channel registered for listening.");
    }

    private async Task SetCountActive(bool active)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var channel = await db.FindChannelOrCreateNewAsync(Context.Channel.Id);
        var countSettings = await db.FindCountSettingsOrCreateNewAsync(Context.Channel.Id);
        countSettings.Channel = channel;
        var count = await db.FindCountOrCreateNewAsync(Context.Channel.Id);
        count.CountSettings = countSettings;

        countSettings.CountingActive = active;
        
        await db.SaveChangesAsync();
    }

    [Command("restore")]
    [Summary("Restore a count in a channel.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task RestoreAsync([Summary("The count that is to be restored.")] int restoreCount)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var count = await db.FindCountOrCreateNewAsync(Context.Channel.Id);
        var countSettings = await db.FindCountSettingsOrCreateNewAsync(Context.Channel.Id);
        if (!countSettings.CountingActive)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        count.CurrentCount = restoreCount;
        await db.SaveChangesAsync();
        await ReplyAsync($"Restored count to {restoreCount}, next number is {restoreCount + 1}!");
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
        var countSettings = await db.FindCountSettingsOrCreateNewAsync(Context.Channel.Id);
        if (!countSettings.CountingActive)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        countSettings.BanActive = toggle;
        await db.SaveChangesAsync();
        await ReplyAsync($"Set ban on fail to {toggle}");
    }

    
    

    [Command("currentcount")]
    [Summary("Displays the current count in a channel.")]
    [RequireContext(ContextType.Guild)]
    public async Task CurrentCountAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var countSettings = await db.FindCountSettingsOrCreateNewAsync(Context.Channel.Id);
        if (!countSettings.CountingActive)
        {
            await ReplyAsync("This channel isn't being listened to!");
            return;
        }

        var count = await db.FindCountOrCreateNewAsync(Context.Channel.Id);

        await ReplyAsync(
            $"Current count is {count.CurrentCount}, next number is {count.CurrentCount + 1}");
    }
    
}