using ClubBot.Data.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClubBot.Logic.Common;

public class AdminModule : ModuleBase<SocketCommandContext>
{
    private readonly IDbContextFactory<DiscordDbContext> _dbContextFactory;
    private readonly ILogger<AdminModule> _logger;

    public AdminModule(IDbContextFactory<DiscordDbContext> dbContextFactory, ILogger<AdminModule> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
    
    [Command("ban")]
    [Summary("Bans a user from the bot.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task Ban(
        [Summary("The user that should be banned.")]
        SocketGuildUser user)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        if (!db.BannedUsers.Any(bannedUser => bannedUser.UserId == user.Id ))
            db.BannedUsers.Add(new BannedUser(user.Id));

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
    [Summary("Unbans a user from the bot.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner(Group = "AdminOrOwner")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "AdminOrOwner")]
    public async Task Unban(
        [Summary("The user that should be unbanned.")]
        SocketGuildUser user)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var bannedUser = await db.BannedUsers.FirstOrDefaultAsync(bu => bu.UserId == user.Id);
        if (bannedUser == null)
        {
            _logger.LogError("Couldn't find banneduser entry for {Id}", user.Id);
            await ReplyAsync($"Couldn't find banneduser entry for {user.Id}");
            return;
        }

        db.BannedUsers.Remove(bannedUser);
        
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
}