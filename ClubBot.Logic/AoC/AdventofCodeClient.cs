using ClubBot.Data.AoC;

namespace ClubBot.Logic.AoC;

public class AdventofCodeClient : IAdventOfCodeClient
{
    private HttpClient _httpClient;
    
    public DateTime LastUpdate { get; }
    public DateTime NextUpdatePossible { get; }
    
    public Leaderboard GetCurrentLeaderboard()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<LeaderboardCompletion> GetNewCompletions()
    {
        throw new NotImplementedException();
    }

    
}

public interface IAdventOfCodeClient
{
    DateTime LastUpdate { get; }
    DateTime NextUpdatePossible { get; }
    Leaderboard GetCurrentLeaderboard();
    IEnumerable<LeaderboardCompletion> GetNewCompletions();
}
