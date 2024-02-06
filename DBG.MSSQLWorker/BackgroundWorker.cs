#region

using DBG.Infrastructure.Interfaces;

#endregion

namespace DBG.MSSQLWorker;

public class BackgroundWorker
    : IHostedService, IDisposable
{
    private readonly IConnectionManager connectionManager;
    private readonly ILogger<BackgroundWorker> logger;
    private Timer? _timer1;
    private Timer? _timer2;
    private Timer? _timer3;

    public BackgroundWorker(ILogger<BackgroundWorker> logger, IConnectionManager connectionManager)
    {
        this.logger = logger;
        this.connectionManager = connectionManager;
    }

    public void Dispose()
    {
        _timer1?.Dispose();
        _timer2?.Dispose();
        _timer3?.Dispose();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer3 = new Timer(connectionManager.LoadConnections, null, TimeSpan.FromSeconds(15),
            TimeSpan.FromMinutes(1));
        _timer1 = new Timer(connectionManager.ReadDbDynamicState, null, TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));
        _timer2 = new Timer(connectionManager.ReadDbStaticState, null, TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));
        logger.LogInformation("Background worker started.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _ = _timer1?.Change(Timeout.Infinite, 0);
        _ = _timer2?.Change(Timeout.Infinite, 0);
        _ = _timer3?.Change(Timeout.Infinite, 0);
        logger.LogInformation("Background worker stopped.");
        return Task.CompletedTask;
    }
}