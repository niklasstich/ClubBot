using ClubBot.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClubBot.Data.AoC;

public class AoCDbContext : DiscordDbContext
{
    private readonly ILogger<AoCDbContext> _logger;

    public AoCDbContext(ILogger<AoCDbContext> logger) : base(logger)
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureLeaderboardModel(modelBuilder);
        //ConfigureLeaderboardUserModel(modelBuilder);
        //ConfigureLeaderboardCompletionModel(modelBuilder);
    }

    private static void ConfigureLeaderboardModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaderboardRegistration>()
            .Navigation(ldb => ldb.Channel)
            .AutoInclude();
    }
/*
    private static void ConfigureLeaderboardUserModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaderboardMember>()
            .Navigation(ldbUser => ldbUser.LeaderboardRegistration)
            .AutoInclude();
        modelBuilder.Entity<LeaderboardMember>()
            .Navigation(ldbUser => ldbUser.LeaderboardCompletions)
            .AutoInclude();
    }

    private static void ConfigureLeaderboardCompletionModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaderboardCompletion>()
            .Navigation(ldbComp => ldbComp.LeaderboardMember)
            .AutoInclude();
    }
    */
}