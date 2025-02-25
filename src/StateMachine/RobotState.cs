namespace SMCAxisController.StateMachine;

public enum RobotState
{
    Initializing,
    WaitingForInput,
    PoweringOnAllAxis,
    ReturningToOriginAll,
    RunningFlow,
    RunningSequence,
    ResetAxis,
    Error
}