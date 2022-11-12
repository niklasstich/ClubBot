using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClubBot.Data.Common;

public class DiscordDbContext : DbContext
{
    public string DbPath { get; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<BannedUser> BannedUsers { get; set; }
    public DiscordDbContext(ILogger<DiscordDbContext> logger)
    {
        _logger = logger;
        var path = DetermineDbLocation();
        DbPath = Path.Join(path, "countingbot.db");
        _logger.LogDebug("Set DbPath to {DbPath}", DbPath);
    }
    private readonly ILogger<DiscordDbContext> _logger;

    private string DetermineDbLocation()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            return DetermineContainerDbLocation();
        _logger.LogDebug("Running on {OsDescription}", RuntimeInformation.OSDescription);
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    private string DetermineContainerDbLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _logger.LogDebug("Determined to be running in a windows container");
            return Path.Join("C:", "dbdata");
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            throw new ApplicationException("Only windows or linux containers are supported");
        _logger.LogDebug("Determined to be running in a linux container");
        if (!Directory.Exists(Path.Join("/", "dbdata")))
            _logger.LogCritical("Path /dbdata does not exist, but needs to exist in order to persist" +
                                " the database. Please rebuild the container with a volume mounted there");
        return Path.Join("/", "dbdata");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
    
    public async Task<Channel> FindChannelOrCreateNewAsync(ulong id)
    {
        var channel = await Channels.FirstOrDefaultAsync(ch => id == ch.GuildChannelId) ??
                      new Channel { GuildChannelId = id };
        if(!await Channels.ContainsAsync(channel))
            Channels.Add(channel);
        return channel;
    }
}