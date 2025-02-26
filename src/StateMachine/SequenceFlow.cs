namespace SMCAxisController.StateMachine;

public class SequenceFlow
{
    public string Name { get; set; }
    public List<SequenceStep> Steps { get; set; } = new List<SequenceStep>();
}


