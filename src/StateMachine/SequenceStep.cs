namespace SMCAxisController.StateMachine;

public class SequenceStep
{
    // When set, this step refers to a predefined sequence.
    public string SequenceRef { get; set; }
    
    // Alternatively, if this step is composite, it can contain its own nested steps.
    public List<SequenceStep> Steps { get; set; } = new List<SequenceStep>();
}