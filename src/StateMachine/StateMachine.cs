using Microsoft.Extensions.Options;
using MudBlazor;
using SMCAxisController.DataModel;
using SMCAxisController.Hardware;

namespace SMCAxisController.StateMachine;
using Stateless;
using Stateless.Graph;

public class StateMachine : IStateMachine
{
    private readonly ILogger<StateMachine> _logger;
    private readonly StateMachine<RobotState, RobotTrigger> _stateMachine;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly RobotSequences _robotSequences;
    private readonly StateMachine<RobotState, RobotTrigger>.TriggerWithParameters<string> _runFlowTrigger;
    private readonly StateMachine<RobotState, RobotTrigger>.TriggerWithParameters<string> _runSequenceTrigger;
    private StateMachineError _lastError = new StateMachineError(){Severity = ErrorSeverity.NoError};
    public StateMachineError LastError
    {
        get
        {
            StateMachineError error =  _lastError with{ControllersStatus = new ControllersStatus()
            {
                isAllConnected = _isAllConnected,
                isSetupToOrigin = _isAllOrigin,
                isAllPowerOn = _isAllPowerOn,
                isAllNotErrorOrEstop = !_isAlarmOrEstop
            }};
            return error;
        }
        private set => _lastError = value;
    }
    public RobotState State { get; private set; }
    
    private RobotState _lastState;
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

    private bool _canPowerOn => _isAllConnected && !_isAlarmOrEstop;
    private bool _canOriginAll => _canPowerOn && _isAllPowerOn;
    private bool _canRun => _canOriginAll && _isAllOrigin;

    public StateMachine(ILogger<StateMachine> logger, 
        IConnectorsRepository connectorsRepository,
        IOptions<RobotSequences> robotSequences)
    {
        _logger = logger;
        _connectorsRepository = connectorsRepository;
        _robotSequences = robotSequences.Value;
        _stateMachine = new StateMachine<RobotState, RobotTrigger>(() => State, s => State = s);
        _runFlowTrigger = _stateMachine.SetTriggerParameters<string>(RobotTrigger.RunFlow);
        _runSequenceTrigger = _stateMachine.SetTriggerParameters<string>(RobotTrigger.RunSequence);

        ConfigureStateMachineStates(_stateMachine);
        ConfigureStateMachineTransitions(_stateMachine);
        _stateMachine.Activate();
    }
    public bool CanFire(RobotTrigger trigger)
    {
        // not allowed to invoke externally
        if (trigger == RobotTrigger.InvokeError)
            return false;
        
        return _stateMachine.CanFire(trigger);
    }
    public async Task Fire(RobotTrigger trigger)
    {
        // forbidden to invoke externally
        if (trigger == RobotTrigger.InvokeError || trigger == RobotTrigger.RunFlow || trigger == RobotTrigger.RunSequence)
            return;
        
        if(_stateMachine.CanFire(trigger))
            await _stateMachine.FireAsync(trigger);
        else
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = $"Can't fire: {trigger.ToString()} now",
                Advice = $"Check if all conditions are met."
            });
        }
    }
    public async Task FireRunFlow(string name)
    {
        if(_stateMachine.CanFire(_runFlowTrigger.Trigger))
            await _stateMachine.FireAsync(_runFlowTrigger, name);
        else
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = $"Can't fire: {_runFlowTrigger.Trigger.ToString()} now",
                Advice = $"Check if all conditions are met."
            });
        }
        
    }
    public async Task FireRunSequence(string name)
    {
        if(_stateMachine.CanFire(_runSequenceTrigger.Trigger))
            await _stateMachine.FireAsync(_runSequenceTrigger, name);
        InvokeError(new StateMachineError()
        {
            Severity = ErrorSeverity.Warning, 
            Message = $"Can't fire: {_runSequenceTrigger.Trigger.ToString()} now",
            Advice = $"Check if all conditions are met."
        });
    }
    private void ConfigureStateMachineStates(StateMachine<RobotState, RobotTrigger> stateMachine)
    {
        // Order of invoking:
        // - OnTransitioned
        // - OnEntry
        // - FireAsync()
        // - OnTransitionCompleted
        // - OnExit - After next trigger

        stateMachine.Configure(RobotState.Initializing)
            .OnActivate(() => stateMachine.FireAsync(RobotTrigger.WaitForInput))
            .Permit(RobotTrigger.WaitForInput, RobotState.WaitingForInput);

        stateMachine.Configure(RobotState.WaitingForInput)
            .PermitIf(RobotTrigger.PowerOnAll, RobotState.PoweringOnAllAxis, () => _canPowerOn)
            //.PermitIf(RobotTrigger.PowerOnAll, RobotState.Error, () => !_canPowerOn)
            .PermitIf(RobotTrigger.ReturnAllToOrigin, RobotState.ReturningToOriginAll, () => _canOriginAll)
            //.PermitIf(RobotTrigger.ReturnAllToOrigin, RobotState.Error, () => !_canOriginAll)
            .PermitIf(RobotTrigger.RunSequence, RobotState.RunningSequence, () => _canRun)
            //.PermitIf(RobotTrigger.RunSequence, RobotState.Error, () => !_canRun)
            .PermitIf(RobotTrigger.RunFlow, RobotState.RunningFlow, () => _canRun)
            //.PermitIf(RobotTrigger.RunSequence, RobotState.Error, () => !_canRun)
            .Permit(RobotTrigger.InvokeError, RobotState.Error);
        
        stateMachine.Configure(RobotState.ReturningToOriginAll)
            .Permit(RobotTrigger.WaitForInput, RobotState.WaitingForInput)
            .Permit(RobotTrigger.InvokeError, RobotState.Error)
            .OnEntryAsync(async () =>
            {
                await ReturnToOriginAllAxis();
                await _stateMachine.FireAsync(RobotTrigger.WaitForInput);
            });
        
        stateMachine.Configure(RobotState.PoweringOnAllAxis)
            .Permit(RobotTrigger.WaitForInput, RobotState.WaitingForInput)
            .Permit(RobotTrigger.InvokeError, RobotState.Error)
            .OnEntryAsync(async () =>
            {
                await PowerOnAllAxis();
                await _stateMachine.FireAsync(RobotTrigger.WaitForInput);
            });
        
        stateMachine.Configure(RobotState.RunningFlow)
            .Permit(RobotTrigger.WaitForInput, RobotState.WaitingForInput)
            .Permit(RobotTrigger.InvokeError, RobotState.Error)
            .OnEntryFromAsync(_runFlowTrigger, async (flowName) =>
            {
                await RunFlow(flowName);
                await _stateMachine.FireAsync(RobotTrigger.WaitForInput);
            });
        
        stateMachine.Configure(RobotState.RunningSequence)
            .Permit(RobotTrigger.WaitForInput, RobotState.WaitingForInput)
            .Permit(RobotTrigger.InvokeError, RobotState.Error)
            .OnEntryFromAsync(_runSequenceTrigger, async (sequenceName) =>
            {
                await RunSequence(sequenceName);
                await _stateMachine.FireAsync(RobotTrigger.WaitForInput);
            });
        
        stateMachine.Configure(RobotState.Error)
            .Permit(RobotTrigger.ClearError, RobotState.WaitingForInput)
            .OnExitAsync(async () =>
            {
                LastError = new StateMachineError(){Severity = ErrorSeverity.NoError};
            });
        
        // below graph can be viawed at: https://dreampuf.github.io/GraphvizOnline/?engine=dot
        string graph = UmlDotGraph.Format(stateMachine.GetInfo());
    }
    private void ConfigureStateMachineTransitions(StateMachine<RobotState, RobotTrigger> stateMachine)
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
    private async Task RunFlow(string name)
    {
        if (_robotSequences == null)
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = "No sequences found",
                Advice = $"Verify if file {RobotSequences.FILENAME} exists and is not empty."
            });
            return;
        }
        
        var flows = _robotSequences.SequenceFlows[name];
        if (flows == null)
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = $"No flow {name} found.",
                Advice = $"Verify if flow {name} exist in the {RobotSequences.FILENAME} file."
            });
            return;
        }
        
        foreach (var step in flows.Steps)
        {
            if (!_canRun)
            {
                InvokeError(new StateMachineError()
                {
                    Severity = ErrorSeverity.Error, 
                    Message = $"Flow interrupted.",
                    Advice = $"Verify if all conditions to move axis are met"
                });
                return;
            }
            await RunSequence(step.SequenceRef);
        }
    }
    private async Task RunSequence(string name)
    {
        if (_robotSequences == null)
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = "No sequences found",
                Advice = $"Verify if file {RobotSequences.FILENAME} exists and is not empty."
            });
            return;
        }

        var sequence = _robotSequences.DefinedSequences[name];
        if (sequence == null)
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = $"No sequence {name} found.",
                Advice = $"Verify if sequence {name} exist in the {RobotSequences.FILENAME} file."
            });
            return;
        }
        
        foreach (var position in sequence.TargetPositions)
        {
            var connector = _connectorsRepository.GetSmcEthernetIpConnectorByName(position.ActuatorName);
            connector.MovementParameters.MovementMode = position.MovementParameters.MovementMode;
            connector.MovementParameters.Speed = position.MovementParameters.Speed;
            connector.MovementParameters.TargetPosition = position.MovementParameters.TargetPosition;
            connector.MovementParameters.Acceleration = position.MovementParameters.Acceleration;
            connector.MovementParameters.Deceleration = position.MovementParameters.Deceleration;
            connector.MovementParameters.PushingForce = position.MovementParameters.PushingForce;
            connector.MovementParameters.TriggerLv = position.MovementParameters.TriggerLv;
            connector.MovementParameters.PushingSpeed = position.MovementParameters.PushingSpeed;
            connector.MovementParameters.PushingForceForPositioning = position.MovementParameters.PushingForceForPositioning;
            connector.MovementParameters.Area1 = position.MovementParameters.Area1;
            connector.MovementParameters.Area2 = position.MovementParameters.Area2;
            connector.MovementParameters.PositioningWidth = position.MovementParameters.PositioningWidth;
            
            if (!_canRun)
            {
                InvokeError(new StateMachineError()
                {
                    Severity = ErrorSeverity.Error, 
                    Message = $"Sequence interrupted.",
                    Advice = $"Verify if all conditions to move axis are met"
                });
                return;
            }
            
            await connector.GoToPositionNumerical();
        }
    }
    private async Task InvokeError(StateMachineError error)
    {
        LastError = error with {};
        MudBlazor.Severity severity = error.Severity switch
        {
            ErrorSeverity.NoError => Severity.Info,
            ErrorSeverity.Warning => Severity.Warning,
            ErrorSeverity.Error => Severity.Error,
            _ => Severity.Info
        };
        NotifySnackbar(LastError.Message, severity);
        _stateMachine.Fire(RobotTrigger.InvokeError);
    }
}