using Microsoft.Extensions.Options;

namespace Sandbox.Configuration;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOptionsMonitor<ApplicationOptions> _options;

    public Worker(ILogger<Worker> logger, IOptionsMonitor<ApplicationOptions> options)
    {
        _logger = logger;
        _options = options;

        using (_options.OnChange(options => { _logger.LogError("Options changed: {options}", options.Deployment); }))
        {
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running in {deployment} at: {time}", _options.CurrentValue.Deployment,
                DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}