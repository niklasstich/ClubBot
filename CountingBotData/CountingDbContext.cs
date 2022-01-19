using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CountingBotData;

public class CountingDbContext : DbContext
{
    private readonly ILogger<CountingDbContext> _logger;
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Count> Counts { get; set; }
    public string DbPath { get; }

    public CountingDbContext(ILogger<CountingDbContext> logger)
    {
        _logger = logger;
        //TODO: change this to a path we persist via a volume
        string path = DetermineDbLocation();
        DbPath = Path.Join(path, "countingbot.db");
        _logger.LogInformation($"Set DbPath to {DbPath}");
    }

    private string DetermineDbLocation()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogInformation("Determined to be running in a windows container.");
                return Path.Join("C:", "dbdata");
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new ApplicationException("Running in an OSX or BSD container is not supported.");
            _logger.LogInformation("Determined to be running in a linux container.");
            if(!Directory.Exists(Path.Join("/", "dbdata"))) 
                _logger.LogCritical("Path /dbdata does not exist, but needs to exist in order to persist" +
                                    " the database. Please rebuild the container with a volume mounted there.");
            return Path.Join("/", "dbdata");
        }
        _logger.LogInformation($"Running on {RuntimeInformation.OSDescription}.");
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var channelTypeBuilder = builder.Entity<Channel>();
        channelTypeBuilder
            .HasOne(ch => ch.Count)
            .WithOne(count => count.Channel)
            .HasForeignKey<Count>(count => count.ChannelId)
            .IsRequired(true);
        
        channelTypeBuilder
            .Navigation(ch => ch.Count).AutoInclude();
    }
}