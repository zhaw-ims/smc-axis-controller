namespace SMCAxisController.StateMachine;

public record ControllersStatus
{
    public bool IsAllConnected { get; init; }
    public bool IsAllNotErrorOrEstop { get; init; }
    public bool IsAllPowerOn { get; init; }
    public bool IsSetupToOrigin { get; init; }
}