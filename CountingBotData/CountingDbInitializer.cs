using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CountingBotData;

public class CountingDbInitializer
{
    private readonly IDbContextFactory<CountingDbContext> _factory;
    private readonly ILogger<CountingDbInitializer> _logger;
    private bool Initialized { get; set; }

    public CountingDbInitializer(IDbContextFactory<CountingDbContext> dbContextFactory, ILogger<CountingDbInitializer> logger)
    {
        _factory = dbContextFactory;
        _logger = logger;
    }
    
    public async Task InitializeDb()
    {
        if (!Initialized)
        {
            await using var db = await _factory.CreateDbContextAsync();
            await db.Database.EnsureCreatedAsync();
            _logger.LogTrace("Initialized Db via EnsureCreatedAsync.");
        }
    }
}