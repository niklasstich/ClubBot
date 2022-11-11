namespace ClubBotData;

public class Channel
{
    public int ChannelId { get; set; }
    public ulong GuildChannelId { get; set; }
    public ulong GuildId { get; set; }
    public bool BanActive { get; set; }
    public List<BannedUser> BannedUsers { get; set; }
    public Count Count { get; set; }
}