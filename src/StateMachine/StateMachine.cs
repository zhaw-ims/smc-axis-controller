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
                IsAllConnected = _isAllConnected,
                IsSetupToOrigin = _isAllOrigin,
                IsAllPowerOn = _isAllPowerOn,
                IsAllNotErrorOrEstop = !_isAlarmOrEstop
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

    private bool _canPowerOn => _isAllConnected && !_isAlarmOrEstop;
    private bool _canOriginAll => _canPowerOn && _isAllPowerOn;
    private bool _canRun => _canOriginAll && _isAllOrigin;

    public StateMachine(ILogger<StateMachine> logger, 
        IConnectorsRepository connectorsRepository,
        RobotSequences robotSequences)
    {
        _logger = logger;
        _connectorsRepository = connectorsRepository;
        _robotSequences = robotSequences;
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
        else
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning,
                Message = $"Can't fire: {_runSequenceTrigger.Trigger.ToString()} now",
                Advice = $"Check if all conditions are met."
            });
        }
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
                if (string.IsNullOrEmpty(flowName) == true)
                {
                    InvokeError(new StateMachineError()
                    {
                        Severity = ErrorSeverity.Error,
                        Message = $"RunningFlow invoked without parameter.",
                        Advice = $"This must be a bug."
                    });
                    return;
                }

                bool ret = await RunFlow(flowName);
                if(LastError.Severity == ErrorSeverity.NoError && ret)
                {
                    _logger.LogInformation($"Flow {flowName} has been run successfully.");
                    await _stateMachine.FireAsync(RobotTrigger.WaitForInput);
                }
                else
                {
                    _logger.LogInformation($"Failed to run flow {flowName}."); 
                }
            });
        
        stateMachine.Configure(RobotState.RunningSequence)
            .Permit(RobotTrigger.WaitForInput, RobotState.WaitingForInput)
            .Permit(RobotTrigger.InvokeError, RobotState.Error)
            .OnEntryFromAsync(_runSequenceTrigger, async (sequenceName) =>
            {
                if (string.IsNullOrEmpty(sequenceName) == true)
                {
                    InvokeError(new StateMachineError()
                    {
                        Severity = ErrorSeverity.Error,
                        Message = $"RunningSequence invoked without parameter.",
                        Advice = $"This must be a bug."
                    });
                    return;
                }
                    
                bool ret = await RunSequence(sequenceName);
                if(LastError.Severity == ErrorSeverity.NoError && ret)
                {
                    _logger.LogInformation($"Sequence {sequenceName} has been run successfully.");
                    await _stateMachine.FireAsync(RobotTrigger.WaitForInput);
                }
                else
                {
                    _logger.LogInformation($"Failed to run sequence {sequenceName}.");    
                }
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
    private async Task<bool> RunFlow(string name)
    {
        if (_robotSequences == null)
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = "No sequences found",
                Advice = $"Verify if file {RobotSequences.FILENAME} exists and is not empty."
            });
            return false;
        }
        
        _robotSequences.SequenceFlows.TryGetValue(name, out var flow);
        if (flow == null)
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = $"No flow {name} found.",
                Advice = $"Verify if flow {name} exist in the {RobotSequences.FILENAME} file."
            });
            return false;
        }
        
        foreach (var step in flow.Steps)
        {
            if (!_canRun)
            {
                InvokeError(new StateMachineError()
                {
                    Severity = ErrorSeverity.Error, 
                    Message = $"Flow interrupted.",
                    Advice = $"Verify if all conditions to move axis are met"
                });
                return false;
            }
            if(await RunSequence(step.SequenceRef) == false)
            {
                _logger.LogInformation($"Failed to run sequence: {step.SequenceRef}");
                return false;
            }
            else
            {
                _logger.LogInformation($"Sequence {step.SequenceRef} has been run successfully.");
            }
        }
        
        return true;
    }
    private async Task<bool> RunSequence(string name)
    {
        if (_robotSequences == null)
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = "No sequences found",
                Advice = $"Verify if file {RobotSequences.FILENAME} exists and is not empty."
            });
            return false;
        }

        _robotSequences.DefinedSequences.TryGetValue(name, out var sequence);
        if (sequence == null)
        {
            InvokeError(new StateMachineError()
            {
                Severity = ErrorSeverity.Warning, 
                Message = $"No sequence {name} found.",
                Advice = $"Verify if sequence {name} exist in the {RobotSequences.FILENAME} file."
            });
            return false;
        }

        foreach (var position in sequence.TargetPositions)
        {
            var connector = _connectorsRepository.GetSmcEthernetIpConnectorByName(position.ActuatorName);
            if (connector == null)
            {
                InvokeError(new StateMachineError()
                {
                    Severity = ErrorSeverity.Error,
                    Message = $"No controller named: {position.ActuatorName} found.",
                    Advice =
                        $"Verify if name {position.ActuatorName} exists in the appsettings.json file."
                });
                return false;
            }

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
                return false;
            }
            
            await connector.GoToPositionNumerical();
        }

        return true;
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
        _logger.LogInformation(LastError.ToString());
        NotifySnackbar(LastError.Message, severity);
        _stateMachine.Fire(RobotTrigger.InvokeError);
    }
}