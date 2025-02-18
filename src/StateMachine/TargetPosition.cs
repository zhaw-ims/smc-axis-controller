using SMCAxisController.DataModel;

namespace SMCAxisController.StateMachine;

public class TargetPosition
{
    public string ActuatorName { get; set; }
    public MovementParameters MovementParameters {get; set;}
}