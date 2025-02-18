namespace SMCAxisController.StateMachine;

public interface IStateMachine
{
    RobotStates State { get; }
    Task Fire(RobotTriggers trigger);
    event Action<string, MudBlazor.Severity> OnSnackBarMessage;
    event Func<Task> OnChange;
}