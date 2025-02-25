using Microsoft.Extensions.Options;
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
    private readonly RobotSequences _robotSequences;
    private readonly StateMachine<RobotStates, RobotTriggers>.TriggerWithParameters<string> _runFlowTrigger;
    private readonly StateMachine<RobotStates, RobotTriggers>.TriggerWithParameters<string> _runSequenceTrigger;

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

    public StateMachine(ILogger<StateMachine> logger, 
        IConnectorsRepository connectorsRepository,
        IOptions<RobotSequences> robotSequences)
    {
        _logger = logger;
        _connectorsRepository = connectorsRepository;
        _robotSequences = robotSequences.Value;
        _stateMachine = new StateMachine<RobotStates, RobotTriggers>(() => State, s => State = s);
        _runFlowTrigger = _stateMachine.SetTriggerParameters<string>(RobotTriggers.StartFlow);
        _runSequenceTrigger = _stateMachine.SetTriggerParameters<string>(RobotTriggers.StartSequence);

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

    public async Task FireRunFlow(string name)
    {
        if(_stateMachine.CanFire(_runFlowTrigger.Trigger))
            await _stateMachine.FireAsync(_runFlowTrigger, name);    
    }
    public async Task FireRunSequence(string name)
    {
        if(_stateMachine.CanFire(_runSequenceTrigger.Trigger))
            await _stateMachine.FireAsync(_runSequenceTrigger, name);      
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
            .PermitIf(RobotTriggers.ReturnToOriginAllAxis, RobotStates.ReturningToOriginAll,
                () => _isAllPowerOn && !_isAlarmOrEstop)
            .PermitIf(RobotTriggers.StartSequence, RobotStates.RunningSequence,
                () => _isAllPowerOn && _isAllConnected && _isAllOrigin && !_isAlarmOrEstop)
            .PermitIf(RobotTriggers.StartFlow, RobotStates.RunningFlow,
            () => _isAllPowerOn && _isAllConnected && _isAllOrigin && !_isAlarmOrEstop);
        
        stateMachine.Configure(RobotStates.ReturningToOriginAll)
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
        
        stateMachine.Configure(RobotStates.RunningDemoSequence)
            .Permit(RobotTriggers.WaitForInput, RobotStates.WaitingForInput)
            .OnEntryAsync(async () =>
            {
                await RunDemoSequence();
                await _stateMachine.FireAsync(RobotTriggers.WaitForInput);
            });
        
        
        stateMachine.Configure(RobotStates.RunningFlow)
            .Permit(RobotTriggers.WaitForInput, RobotStates.WaitingForInput)
            .OnEntryFromAsync(_runFlowTrigger, async (flowName) =>
            {
                await RunFlow(flowName);
                await _stateMachine.FireAsync(RobotTriggers.WaitForInput);
        });
        
        stateMachine.Configure(RobotStates.RunningSequence)
            .Permit(RobotTriggers.WaitForInput, RobotStates.WaitingForInput)
            .OnEntryFromAsync(_runSequenceTrigger, async (sequenceName) =>
            {
                await RunSequence(sequenceName);
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
    private async Task RunFlow(string name)
    {
        if (_robotSequences == null)
        {
            NotifySnackbar("No Robot Sequences found!", MudBlazor.Severity.Error);
            return;
        }
        
        var flows = _robotSequences.SequenceFlows[name];

        foreach (var step in flows.Steps)
        {
            await RunFlowStep(step);
        }
    }
    private async Task RunFlowStep(SequenceStep step)
    {
        if (step.IsComposite)
        {
            // The step contains nested steps.
            foreach (var nestedStep in step.Steps)
            {
                await RunFlowStep(nestedStep);
            }
        }
        else if (step.IsLeaf)
        {
            // The step is a leaf and directly references a sequence.
            await RunSequence(step.SequenceRef);
        }
        else
        {
            // Optional: handle steps that neither reference a sequence nor contain nested steps.
            NotifySnackbar("Invalid sequence step configuration!", MudBlazor.Severity.Error);
        }
    }
    private async Task RunSequence(string name)
    {
        if (_robotSequences == null)
        {
            NotifySnackbar("No Robot Sequences found!", MudBlazor.Severity.Error);
            return;
        }

        var sequence = _robotSequences.DefinedSequences[name];
        
        if (sequence == null)
        {
            NotifySnackbar("No Move Sequences found!", MudBlazor.Severity.Warning);
            return;
        }
        
        foreach (var position in sequence.TargetPositions)
        {
            var connector = _connectorsRepository.GetSmcEthernetIpConnectorByName(position.ActuatorName);
            connector.MovementParameters.Speed = position.MovementParameters.Speed;
            connector.MovementParameters.TargetPosition = position.MovementParameters.TargetPosition;
            await connector.GoToPositionNumerical();
        }
    }
}