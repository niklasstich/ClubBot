using Discord;
using Discord.Commands;

namespace CountingBotLogic;

public class DebugModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("Responds with a message.")]
    [RequireOwner(Group = "permission")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "permission")]
    public Task PingAsync() => ReplyAsync("Pong!");
    
    
}
