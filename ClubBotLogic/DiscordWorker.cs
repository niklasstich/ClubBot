using ClubBotData;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClubBotLogic;

public class DiscordWorker : BackgroundService
{
    private readonly ILogger<DiscordWorker> _logger;
    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _commandHandler;
    private CountingDbInitializer? _dbInitializer;

    public DiscordWorker(ILogger<DiscordWorker> logger, DiscordSocketClient client, CommandHandler commandHandler,
        CountingDbInitializer? dbInitializer)
    {
        _logger = logger;
        _client = client;
        _commandHandler = commandHandler;
        _dbInitializer = dbInitializer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting bot...");
        var discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

        _client.Log += Log;
        _client.Connected += () =>
        {
            _logger.LogInformation("Connected to discord gateway.");
            return Task.CompletedTask;
        };
        
        await _dbInitializer?.InitializeDb()!;
        await _commandHandler.InstallCommandsAsync();
        
        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();
        await Task.Delay(-1, stoppingToken);
    }

    private Task Log(LogMessage msg)
    {
        switch (msg.Severity)
        {
            case LogSeverity.Critical:
                _logger.LogCritical("{}", msg.Message);
                break;
            case LogSeverity.Error:
                _logger.LogError("{}", msg.Message);
                break;
            case LogSeverity.Warning:
                _logger.LogWarning("{}", msg.Message);
                break;
            case LogSeverity.Info:
                _logger.LogInformation("{}", msg.Message);
                break;
            case LogSeverity.Verbose:
                _logger.LogDebug("{}", msg.Message);
                break;
            case LogSeverity.Debug:
                _logger.LogTrace("{}", msg.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(msg), "unknown LogSeverity level " + msg.Severity);
        }
        return Task.CompletedTask;
    }
}