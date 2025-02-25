namespace SMCAxisController.StateMachine;

public interface IStateMachine
{
    RobotState State { get; }
    Task Fire(RobotTrigger trigger);
    bool CanFire(RobotTrigger trigger);
    Task FireRunFlow(string name);
    Task FireRunSequence(string name);
    event Action<string, MudBlazor.Severity> OnSnackBarMessage;
    event Func<Task> OnChange;
    StateMachineError LastError { get; }
}