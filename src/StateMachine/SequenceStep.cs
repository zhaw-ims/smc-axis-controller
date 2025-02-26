namespace SMCAxisController.StateMachine;

/// <summary>
/// Represents a single step in a sequence flow for robot movement control.
/// A step references a predefined sequence by name that contains movement instructions.
/// </summary>
public class SequenceStep 
{
    /// <summary>
    /// Gets or sets the name of a predefined sequence to execute.
    /// The sequence name must match a predefined sequence in the RobotSequences configuration.
    /// </summary>
    public string SequenceRef { get; set; }
}