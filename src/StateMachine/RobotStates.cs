namespace SMCAxisController.StateMachine;

public enum RobotStates
{
    Initializing,
    WaitingForInput,
    PoweringOnAllAxis,
    ReturningToOriginAll,
    RunningDemoSequence,
    RunningFlow,
    RunningSequence
}