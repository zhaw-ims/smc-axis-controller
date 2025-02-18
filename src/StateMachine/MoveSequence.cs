namespace SMCAxisController.StateMachine;

public class MoveSequence
{
    public string Name { get; set; }
    public IEnumerable<TargetPosition> TargetPositions { get; set; } = new List<TargetPosition>();
}