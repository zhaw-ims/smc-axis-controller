using SMCAxisController.DataModel;

namespace SMCAxisController.Hardware;

public class SmcRegisterPollingBackgroundService : BackgroundService
{
    private readonly ILogger<SmcRegisterPollingBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConnectorsRepository _connectorsRepository;

    public SmcRegisterPollingBackgroundService(ILogger<SmcRegisterPollingBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConnectorsRepository connectorsRepository)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _connectorsRepository = connectorsRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Create a new scope for the lifetime of this background service.
        var scope = _serviceScopeFactory.CreateScope();
        
        _connectorsRepository.SmcEthernetIpConnectors = scope.ServiceProvider.GetRequiredService<IEnumerable<ISmcEthernetIpConnector>>().ToList();
        
        if (!_connectorsRepository.SmcEthernetIpConnectors.Any())
        {
            _logger.LogWarning("No connectors were found.");
            return;
        }
        
        // Wait for initialization of gui
        await Task.Delay(2000, cancellationToken);
        
        // First, attempt to connect all connectors concurrently.
        var connectionTasks = _connectorsRepository.SmcEthernetIpConnectors.Select(connector => ConnectUntilSuccessfulAsync(connector, cancellationToken));
        await Task.WhenAll(connectionTasks);
        
        // Once all connectors are connected, start polling data concurrently.
        var pollingTasks = _connectorsRepository.SmcEthernetIpConnectors.Select(connector => PollConnectorDataAsync(connector, cancellationToken));
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
                else
                {
                    await Task.Delay(1000, cancellationToken);    
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