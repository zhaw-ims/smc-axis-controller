namespace SMCAxisController.StateMachine;

public class RobotSequences
{
    public const string KEY = "RobotSequences";
    public const string FILENAME = "robotsequences.json";
    // public IEnumerable<MoveSequence> MoveSequences { get; set; } = new List<MoveSequence>();
    public Dictionary<string, MoveSequence> DefinedSequences { get; set; } = new Dictionary<string, MoveSequence>();
    //public IEnumerable<SequenceFlow> SequenceFlows { get; set; } = new List<SequenceFlow>();
    public Dictionary<string, SequenceFlow> SequenceFlows { get; set; } = new Dictionary<string, SequenceFlow>();
    public SamplesGrid SamplesGrid { get; set; } = new SamplesGrid();
    public SequenceFlow GeneratedFlowPattern { get; set; } = new SequenceFlow();
}