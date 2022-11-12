
namespace ClubBot.Data.Counting;

public class Count
{
    public int CountId { get; set; }
    public int CurrentCount { get; set; }
    public int MaxCount { get; set; }
    public ulong? LastUserId { get; set; }
    public ulong? MaxUserId { get; set; }
    public DateTime LastCountTime { get; set; }
    public DateTime MaxCountTime { get; set; }
    public CountSettings CountSettings { get; set; }
}