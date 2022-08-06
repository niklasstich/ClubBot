using Discord.Commands;

namespace CountingBotLogic;

public class CooldownAttribute : PreconditionAttribute
{
    private readonly int _seconds;
    private DateTime _lastUsed;
    public CooldownAttribute(int seconds)
    {
        _seconds = seconds;
        _lastUsed = DateTime.UnixEpoch;
    }
    
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (DateTime.Now <= _lastUsed.Add(TimeSpan.FromSeconds(_seconds)))
            return Task.FromResult(PreconditionResult.FromError($"Command on cooldown - please wait {((_lastUsed.AddSeconds(_seconds))-DateTime.Now).Seconds} seconds."));
        _lastUsed = DateTime.Now;
        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}