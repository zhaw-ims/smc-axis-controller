namespace SMCAxisController.StateMachine;

public class SequenceStep
{
    // When set, this step refers to a predefined sequence.
    public string SequenceRef { get; set; }
    
    // Alternatively, if this step is composite, it can contain its own nested steps.
    public List<SequenceStep> Steps { get; set; } = new List<SequenceStep>();
    
    // Returns true if this step is a leaf (has a reference) and not nested.
    public bool IsLeaf => !string.IsNullOrWhiteSpace(SequenceRef) && (Steps == null || Steps.Count == 0);

    // Returns true if this step contains nested steps.
    public bool IsComposite => Steps != null && Steps.Count > 0;
}