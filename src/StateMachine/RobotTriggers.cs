
namespace SMCAxisController.StateMachine;
public enum RobotTriggers
{
    Initialize,
    WaitForInput,
    ReturnToOriginAllAxis,
    PowerOnAllAxis,
    StartDemoSequence,
    StartSequence, // trigger with parameter
    StartFlow, // trigger with parameter
}