namespace SMCAxisController.StateMachine;

public class RobotSequences
{
    public const string KEY = "RobotSequences";
    public const string FILENAME = "robotsequences.json";
    public Dictionary<string, MoveSequence> DefinedSequences { get; set; } = new Dictionary<string, MoveSequence>();
    public Dictionary<string, SequenceFlow> SequenceFlows { get; set; } = new Dictionary<string, SequenceFlow>();
    public SamplesGrid SamplesGrid { get; set; } = new SamplesGrid();
    public SequenceFlow GeneratedFlowTemplate { get; set; } = new SequenceFlow();
}