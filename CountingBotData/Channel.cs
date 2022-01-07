namespace CountingBotData;

public class Channel
{
    public int ChannelId { get; set; }
    public ulong GuildChannelId { get; set; }
    public ulong GuildId { get; set; }
    public ulong BanRoleId { get; set; }
    public bool BanRoleActive { get; set; }
    //TODO: make an entity and a list of entities here for banned users
    //public List<ulong> ManuallyBannedUsers { get; set; }
    public Count Count { get; set; }
}