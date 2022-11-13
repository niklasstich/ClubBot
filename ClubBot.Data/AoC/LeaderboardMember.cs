using System.Text.Json.Serialization;

namespace ClubBot.Data.AoC;

public class LeaderboardMember
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("local_score")]
    public int LocalScore { get; set; }
    [JsonPropertyName("stars")]
    public int Stars { get; set; }
    [JsonPropertyName("last_star_ts")]
    public DateTime? LastStar { get; set; }
    [JsonPropertyName("completion_day_level")]
    public IEnumerable<LeaderboardCompletion> LeaderboardCompletions { get; set; }
}