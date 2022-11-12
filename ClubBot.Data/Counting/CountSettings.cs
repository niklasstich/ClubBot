using ClubBot.Data.Common;

namespace ClubBot.Data.Counting;

public class CountSettings
{
    public int CountSettingsId { get; set; }
    public bool CountingActive { get; set; }
    public bool BanActive { get; set; }
    public int ChannelId { get; set; }
    public Channel Channel { get; set; }
}