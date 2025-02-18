namespace SMCAxisController.StateMachine;

public record MoveSequence
{
    public string Name { get; set; }
    public List<TargetPosition> TargetPositions { get; set; } = new List<TargetPosition>();
}