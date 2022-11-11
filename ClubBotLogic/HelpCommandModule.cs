using System.Text;
using Discord;
using Discord.Commands;

namespace ClubBotLogic;

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
            stringBuilder.AppendLine($"Name: {commmandInfo.Name}");
            stringBuilder.AppendLine($"Summary: {commmandInfo.Summary}");
            if (commmandInfo.Preconditions.Any(p => p != null && p.Group != null))
            {
                var distinctPreconditions = commmandInfo.Preconditions
                    .Select(p => p.Group)
                    .Where(p => p != null)
                    .Distinct()
                    .ToList();
                stringBuilder.AppendLine($"Conditions: {string.Join(", ", distinctPreconditions)}");
            }

            if (commmandInfo.Parameters.Any(p => p != null))
            {
                stringBuilder.Append("Parameters: ");
                foreach (var parameterInfo in commmandInfo.Parameters)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append($"\t{parameterInfo.Name}: {parameterInfo.Summary} ");
                }
                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
        }

        try
        {
            await Context.User.SendMessageAsync(stringBuilder.ToString());
        }
        catch (Exception e)
        {
            await ReplyAsync("Couldn't send you a DM with the commands. Please make sure you have DMs enabled.");
            return;
        }
        await ReplyAsync("Check your DMs!");
    }
}