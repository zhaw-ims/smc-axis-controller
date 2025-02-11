namespace SMCAxisController.StateMachine;

public class IStateMachine
{
    RobotStates RobotState { get; }
    event Func<Task> OnChange;
}