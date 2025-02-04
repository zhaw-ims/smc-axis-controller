using SMCAxisController.DataModel;

namespace SMCAxisController.Hardware;

public class SmcRegisterPollingBackgroundService : BackgroundService
{
    private readonly ILogger<SmcRegisterPollingBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public SmcRegisterPollingBackgroundService(ILogger<SmcRegisterPollingBackgroundService> logger, 
            IServiceScopeFactory serviceScopeFactory) {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken);
        
        // Create a new scope for the lifetime of this background service.
        using var scope = _serviceScopeFactory.CreateScope();
        
        var connectorFactory = scope.ServiceProvider.GetRequiredService<ISmcEthernetIpConnectorFactory>();
        
        var connectors = connectorFactory.GetAllSmcEthernetIpConnectors();
        
        if (!connectors.Any())
        {
            _logger.LogWarning("No connectors were found.");
            return;
        }
        
        // First, attempt to connect all connectors concurrently.
        var connectionTasks = connectors.Select(connector => ConnectUntilSuccessfulAsync(connector, cancellationToken));
        await Task.WhenAll(connectionTasks);
        
        // Once all connectors are connected, start polling data concurrently.
        var pollingTasks = connectors.Select(connector => PollConnectorDataAsync(connector, cancellationToken));
        await Task.WhenAll(pollingTasks);
    }

    private async Task ConnectUntilSuccessfulAsync(ISmcEthernetIpConnector connector, CancellationToken cancellationToken)
    {
        while (connector.Status != ControllerStatus.Connected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation($"Attempting to connect: {connector.ControllerProperties.Name}...");
                connector.Connect();
                
                if (connector.Status == ControllerStatus.Connected)
                {
                    _logger.LogInformation($"Successfully connected: {connector.ControllerProperties.Name}!");
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Connection attempt for {connector.ControllerProperties.Name} failed: {ex.Message}");
            }
            
            await Task.Delay(100, cancellationToken);
        }
    }

    private async Task PollConnectorDataAsync(ISmcEthernetIpConnector connector, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                connector.GetData();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting data from {connector.ControllerProperties.Name}: {ex.Message}");
            }
            
            await Task.Delay(100, cancellationToken);
        }
    }
}