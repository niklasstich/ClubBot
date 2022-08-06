using Discord;
using Discord.Commands;

namespace CountingBotLogic;

public class DebugModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("Responds with a message.")]
    [Cooldown(60)]
    public Task PingAsync() => ReplyAsync("Pong!");
    
    
}
