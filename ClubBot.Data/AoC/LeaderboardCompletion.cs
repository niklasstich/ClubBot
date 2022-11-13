namespace ClubBot.Data.AoC;

public class LeaderboardCompletion
{
    public int Day { get; set; }
    public Part Part { get; set; }
    public DateTime CompletionTime { get; set; }
}


public enum Part
{
    PartOne = 1,
    PartTwo = 2
}

