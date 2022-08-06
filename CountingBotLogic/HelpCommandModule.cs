using System.Text;
using Discord.Commands;

namespace CountingBotLogic;

public class HelpCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly CommandService _commandService;

    public HelpCommandModule(CommandService commandService)
    {
        _commandService = commandService;
    }

    [Command("help")]
    [Summary("Displays all commands")]
    [RequireContext(ContextType.Guild)]
    [Cooldown(60)]
    public async Task PrintHelpAsync()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("List of all commands:");
        foreach (var commmandInfo in _commandService.Commands)
        {
            stringBuilder.AppendLine($"Name: {commmandInfo.Name},\t Summary: {commmandInfo.Summary}\t");
            if (commmandInfo.Preconditions.Any())
            {
                var distinctPreconditions = commmandInfo.Preconditions
                    .Select(p => p.Group)
                    .Distinct();
                stringBuilder.Append($"Preconditions: {string.Join(", ", distinctPreconditions)}");
            }

            stringBuilder.AppendLine();
        }

        await ReplyAsync(stringBuilder.ToString());
    }
}