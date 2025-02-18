namespace SMCAxisController.StateMachine;

public enum RobotStates
{
    Initializing,
    WaitingForInput,
    PoweringOnAllAxis,
    WaitingForReturnToOriginAll,
    WaitingForX,
    WaitingForY,
    WaitingForZ,
    WaitForGripper,
    DemoSequence
}