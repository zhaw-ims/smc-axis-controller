using SMCAxisController.DataModel;
using SMCAxisController.Hardware;

namespace SMCAxisController.StateMachine;
using Stateless;
using Stateless.Graph;

public class StateMachine : IStateMachine
{
    private readonly ILogger<StateMachine> _logger;
    private readonly StateMachine<RobotStates, RobotTriggers> _stateMachine;
    private readonly IConnectorsRepository _connectorsRepository;

    public RobotStates State { get; private set; }
    
    private RobotStates _lastState;
    public event Func<Task> OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();
    public event Action<string, MudBlazor.Severity> OnSnackBarMessage;
    private void NotifySnackbar(string message, MudBlazor.Severity severity = MudBlazor.Severity.Normal) => OnSnackBarMessage?.Invoke(message, severity);
    private bool _isAllPowerOn => _connectorsRepository.SmcEthernetIpConnectors
        .All(connector => connector.ControllerInputData.IsSvre());
    private bool _isAllConnected => _connectorsRepository.SmcEthernetIpConnectors
        .All(connector => connector.Status == ControllerStatus.Connected);
    
    private bool _isAllOrigin => _connectorsRepository.SmcEthernetIpConnectors
        .All(connector => connector.ControllerInputData.IsReady());
    
    private bool _isAlarmOrEstop => _connectorsRepository.SmcEthernetIpConnectors
        .All(connector => connector.ControllerInputData.IsAlarm() || connector.ControllerInputData.IsEstop());
    private int _controllersCount => _connectorsRepository.SmcEthernetIpConnectors.Count;

    public StateMachine(ILogger<StateMachine> logger, IConnectorsRepository connectorsRepository)
    {
        _logger = logger;
        _connectorsRepository = connectorsRepository;
        _stateMachine = new StateMachine<RobotStates, RobotTriggers>(() => State, s => State = s);
        ConfigureStateMachineStates(_stateMachine);
        ConfigureStateMachineTransitions(_stateMachine);
        _stateMachine.Activate();
    }
    public async Task Fire(RobotTriggers trigger) {
        if(_stateMachine.CanFire(trigger))
            await _stateMachine.FireAsync(trigger);
        else
        {
            NotifySnackbar($"Can't fire: {trigger.ToString()} now", MudBlazor.Severity.Warning);
        }
    }
    private void ConfigureStateMachineStates(StateMachine<RobotStates, RobotTriggers> stateMachine)
    {
        // Order of invoking:
        // - OnTransitioned
        // - OnEntry
        // - FireAsync()
        // - OnTransitionCompleted
        // - OnExit - After next trigger

        stateMachine.Configure(RobotStates.Initializing)
            .OnActivate(() => stateMachine.FireAsync(RobotTriggers.WaitForInput))
            .Permit(RobotTriggers.WaitForInput, RobotStates.WaitingForInput);

        stateMachine.Configure(RobotStates.WaitingForInput)
            .PermitIf(RobotTriggers.PowerOnAllAxis, RobotStates.PoweringOnAllAxis,
                () => _isAllConnected && !_isAlarmOrEstop)
            .PermitIf(RobotTriggers.ReturnToOriginAllAxis, RobotStates.WaitingForReturnToOriginAll,
                () => _isAllPowerOn && !_isAlarmOrEstop)
            .PermitIf(RobotTriggers.StartDemoSequence, RobotStates.DemoSequence,
                () => _isAllPowerOn && _isAllConnected && _isAllOrigin && !_isAlarmOrEstop);
        
        stateMachine.Configure(RobotStates.WaitingForReturnToOriginAll)
            .Permit(RobotTriggers.WaitForInput, RobotStates.WaitingForInput)
            .OnEntryAsync(async () =>
            {
                await ReturnToOriginAllAxis();
                await _stateMachine.FireAsync(RobotTriggers.WaitForInput);
            });
        
        stateMachine.Configure(RobotStates.PoweringOnAllAxis)
            .Permit(RobotTriggers.WaitForInput, RobotStates.WaitingForInput)
            .OnEntryAsync(async () =>
            {
                await PowerOnAllAxis();
                await _stateMachine.FireAsync(RobotTriggers.WaitForInput);
            });
        
        stateMachine.Configure(RobotStates.DemoSequence)
            .Permit(RobotTriggers.WaitForInput, RobotStates.WaitingForInput)
            .OnEntryAsync(async () =>
            {
                await RunDemoSequence();
                await _stateMachine.FireAsync(RobotTriggers.WaitForInput);
            });
        
        // below graph can be viawed at: https://dreampuf.github.io/GraphvizOnline/?engine=dot
        string graph = UmlDotGraph.Format(stateMachine.GetInfo());
    }
    private void ConfigureStateMachineTransitions(StateMachine<RobotStates, RobotTriggers> stateMachine)
    {    
        stateMachine.OnTransitioned(transition => {
            _lastState = transition.Source;
            _logger.LogDebug($"Transition from {_lastState} to {transition.Destination}");
            _logger.LogDebug("Transitioned from {SourceState} to {DestinationState} via {Trigger}", transition.Source,
                transition.Destination, transition.Trigger);
        });

        stateMachine.OnTransitionCompleted(transition => {
            _logger.LogDebug($"Transition from {_lastState} to {transition.Destination}");
            _logger.LogDebug("Transition completed from {SourceState} to {DestinationState} via {Trigger}",
                transition.Source,
                transition.Destination, transition.Trigger);
      
            NotifyStateChanged();
        });
    }
    private async Task ReturnToOriginAllAxis()
    {
        foreach (var connector in _connectorsRepository.SmcEthernetIpConnectors)
        {
            await connector.ReturnToOrigin();
        }
    }
    private async Task PowerOnAllAxis()
    {
        foreach (var connector in _connectorsRepository.SmcEthernetIpConnectors)
        {
            connector.PowerOn();
        }
    }
    private async Task RunDemoSequence()
    {
        foreach (var connector in _connectorsRepository.SmcEthernetIpConnectors)
        {
            connector.MovementParameters.Speed = 20;
            connector.MovementParameters.TargetPosition = 500;
            await connector.GoToPositionNumerical();
        }
    }
    private async Task RunSequence(string name)
    {
        List<MoveSequence> sequences = new List<MoveSequence>();
        
        var sequence = sequences.SingleOrDefault(s => s.Name == name);
        
        foreach (var position in sequence.TargetPositions)
        {
            var connector = _connectorsRepository.GetSmcEthernetIpConnectorByName(position.ActuatorName);
            connector.MovementParameters.Speed = position.Speed;
            connector.MovementParameters.TargetPosition = position.Position;
            await connector.GoToPositionNumerical();
        }
    }
}