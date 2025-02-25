namespace SMCAxisController.StateMachine;

public record ControllersStatus
{
    public bool isAllConnected { get; init; }
    public bool isAllNotErrorOrEstop { get; init; }
    public bool isAllPowerOn { get; init; }
    public bool isSetupToOrigin { get; init; }
}