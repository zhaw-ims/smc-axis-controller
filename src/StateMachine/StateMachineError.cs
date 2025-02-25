namespace SMCAxisController.StateMachine;

public record StateMachineError
{
    public string Message { get; init; } = "No Error";
    public string Advice { get; init; } = "-";
    public ErrorSeverity Severity { get; init; } = ErrorSeverity.NoError;
    public ControllersStatus? ControllersStatus { get; init; } = null;
}