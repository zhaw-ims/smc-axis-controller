namespace SMCAxisController.StateMachine;
using Stateless;
using Stateless.Graph;

public class StateMachine : IStateMachine
{
    private readonly ILogger<StateMachine> _logger;
    private readonly StateMachine<RobotStates, RobotTriggers> _stateMachine;
    public RobotStates RobotState { get; private set; }
    private RobotStates _lastRobotState;
    private void NotifyStateChanged() => OnChange?.Invoke();
    public event Func<Task> OnChange;

    public StateMachine(ILogger<StateMachine> logger)
    {
        _stateMachine = new StateMachine<RobotStates, RobotTriggers>(() => RobotState, s => RobotState = s);
        ConfigureStateMachineStates(_stateMachine);
        ConfigureStateMachineTransitions(_stateMachine);
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
            .OnActivate(() => stateMachine.FireAsync(RobotTriggers.EnterStateMachine))
            .Permit(RobotTriggers.EnterStateMachine, RobotStates.WaitingForInput);
        
        stateMachine.Configure(RobotStates.WaitingForInput)
            .Permit(RobotTriggers.ReturnToOrgin, RobotStates.WaitingForReturnToOriginGripper)
            .OnEntryAsync(async () => await ReturnToOriginGripper());
    }
    private void ConfigureStateMachineTransitions(StateMachine<RobotStates, RobotTriggers> stateMachine)
    {    
        stateMachine.OnTransitioned(transition => {
            _lastRobotState = transition.Source;
            _logger.LogDebug($"Transition from {_lastRobotState} to {transition.Destination}");
            _logger.LogDebug("Transitioned from {SourceState} to {DestinationState} via {Trigger}", transition.Source,
                transition.Destination, transition.Trigger);
        });

        stateMachine.OnTransitionCompleted(transition => {
            _logger.LogDebug($"Transition from {_lastRobotState} to {transition.Destination}");
            _logger.LogDebug("Transition completed from {SourceState} to {DestinationState} via {Trigger}",
                transition.Source,
                transition.Destination, transition.Trigger);
      
            NotifyStateChanged();
        });
    }

    async Task ReturnToOriginGripper()
    {
        
    }
}