using ClubBot.Data.Common;

namespace ClubBot.Data.AoC;

public class LeaderboardRegistration
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int LeaderboardRegistrationId { get; set; }
    /// <summary>
    /// Actual adventofcode.com leaderboard id
    /// </summary>
    public int AoCLeaderboardId { get; set; }
    /// <summary>
    /// The channel to which this leaderboard is registered 
    /// </summary>
    public Channel Channel { get; set; }
    public int Year { get; set; }
    public DateTime LastUpdate { get; set; }
}