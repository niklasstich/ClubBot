namespace ClubBot.Data.Common;

public class BannedUser
{
    public BannedUser(ulong userId)
    {
        UserId = userId;
    }
    
    public int BannedUserId { get; set; }
    public ulong UserId { get; set; }
}