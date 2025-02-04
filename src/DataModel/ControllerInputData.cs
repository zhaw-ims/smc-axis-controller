namespace SMCAxisController.DataModel;

public class ControllerInputData
{
    public int InputPort { get; set; }
    public int ControllerInformationFlag { get; set; }
    public int CurrentPosition { get; set; }
    public int CurrentSpeed { get; set; }
    public int CurrentPushingForce { get; set; }
    public int TargetPosition { get; set; }
    public int Alarm1And2 { get; set; }
    public int Alarm3And4 { get; set; }
}