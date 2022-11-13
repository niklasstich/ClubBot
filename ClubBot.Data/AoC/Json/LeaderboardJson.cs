using System.Text.Json.Serialization;

namespace ClubBot.Data.AoC.Json;

public class LeaderboardJson
{
    [JsonPropertyName("members")]
    public Dictionary<string,LeaderboardMemberJson> Members { get; set; }
    [JsonPropertyName("owner_id")]
    public string Owner { get; set; }
    [JsonPropertyName("event")]
    public string Year { get; set; }

    public Leaderboard ToProper() =>
        new()
        {
            Owner = int.Parse(Owner),
            Year = int.Parse(Year),
            Members = Members.Select(lmj => lmj.Value.ToProper())
        };
}

public class LeaderboardMemberJson
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("local_score")]
    public int LocalScore { get; set; }
    [JsonPropertyName("stars")]
    public int Stars { get; set; }
    [JsonPropertyName("last_star_ts")]
    public object LastStar { get; set; }
    [JsonPropertyName("completion_day_level")]
    public Dictionary<string,Dictionary<string,Dictionary<string, int>>> LeaderboardCompletions { get; set; }

    public LeaderboardMember ToProper() =>
        new()
        {
            Id = int.Parse(Id),
            Name = Name ?? $"Anonymous {Id}",
            LocalScore = LocalScore,
            Stars = Stars,
            LastStar = LastStar is "0" ? null : new DateTime((long)LastStar),
            LeaderboardCompletions = LeaderboardCompletions.SelectMany(kv => kv.Value.Select(kv2 =>
            {
                var completionTime = new DateTime(kv2.Value["get_star_ts"]);
                var dayParsed = int.Parse(kv.Key);
                var partParsed = (Part)int.Parse(kv2.Key);
                return new LeaderboardCompletion { CompletionTime = completionTime, Day = dayParsed, Part = partParsed };
            }))
        };
}