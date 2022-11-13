using System.Text.Json.Serialization;

namespace ClubBot.Data.AoC;

public class Leaderboard
{
    [JsonPropertyName("members")]
    public IEnumerable<LeaderboardMember> Members { get; set; }
    [JsonPropertyName("owner_id")]
    public int Owner { get; set; }
    [JsonPropertyName("event")]
    public int Year { get; set; }
}