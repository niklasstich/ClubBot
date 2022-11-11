namespace ClubBotData;

public class BannedUser
{
    public BannedUser(int channelId, ulong userId)
    {
        ChannelId = channelId;
        UserId = userId;
    }
    
    public int BanId { get; set; }
    public int ChannelId { get; set; }
    public Channel Channel { get; set; }
    public ulong UserId { get; set; }
}