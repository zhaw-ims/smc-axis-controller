namespace SMCAxisController.StateMachine;

public record TargetPosition
{
    public string ActuatorName { get; set; }
    public int Speed{ get; set; }
    public int Position { get; set; }
}