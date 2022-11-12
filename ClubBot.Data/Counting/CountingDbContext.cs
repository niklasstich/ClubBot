using ClubBot.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClubBot.Data.Counting;

public class CountingDbContext : DiscordDbContext
{
    public DbSet<Count> Counts { get; set; }
    public DbSet<CountSettings> CountSettings { get; set; }

    public CountingDbContext(ILogger<CountingDbContext> logger) : base(logger)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder
            .Entity<Count>()
            .Navigation(co => co.CountSettings)
            .AutoInclude();
        modelBuilder
            .Entity<CountSettings>()
            .Navigation(cs => cs.Channel)
            .AutoInclude();
        modelBuilder
            .Entity<CountSettings>();
    }

    public async Task<CountSettings> FindCountSettingsOrCreateNewAsync(ulong id)
    {
        var countSettings = await CountSettings.FirstOrDefaultAsync(cs => id == cs.Channel.GuildChannelId) ??
                            new CountSettings();
        if (!await CountSettings.ContainsAsync(countSettings))
            CountSettings.Add(countSettings);
        return countSettings;
    }
    public async Task<Count> FindCountOrCreateNewAsync(ulong id)
    {
        var count = await Counts.FirstOrDefaultAsync(cs => id == cs.CountSettings.Channel.GuildChannelId) ??
                            new Count();
        if(!await Counts.ContainsAsync(count))
            Counts.Add(count);
        return count;
    }
}