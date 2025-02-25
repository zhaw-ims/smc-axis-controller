namespace SMCAxisController.StateMachine;

public interface IStateMachine
{
    RobotStates State { get; }
    Task Fire(RobotTriggers trigger);
    Task FireRunFlow(string name);
    Task FireRunSequence(string name);
    event Action<string, MudBlazor.Severity> OnSnackBarMessage;
    event Func<Task> OnChange;
}