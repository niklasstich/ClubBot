using Microsoft.EntityFrameworkCore;

namespace CountingBotData;

public class CountingDbContext : DbContext
{
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Count> Counts { get; set; }
    public string DbPath { get; }

    public CountingDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "countingbot.db");
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