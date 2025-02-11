namespace SMCAxisController.StateMachine;

public enum RobotStates
{
    Initializing,
    WaitingForInput,
    WaitingForReturnToOriginGripper,
    WaitingForReturnToOriginX,
    WaitingForReturnToOriginY,
    WaitingForReturnToOriginZ,
}