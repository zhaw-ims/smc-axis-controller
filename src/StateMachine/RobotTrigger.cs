
namespace SMCAxisController.StateMachine;
public enum RobotTrigger
{
    Initialize,
    WaitForInput,
    ReturnAllToOrigin,
    PowerOnAll,
    ResetAxis,
    RunSequence, // trigger with parameter
    RunFlow, // trigger with parameter
    InvokeError,
    ClearError,
}