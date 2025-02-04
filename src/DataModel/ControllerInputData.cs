using System.Text;

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
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Inputs:");
        sb.AppendLine($"InputPort: {InputPort}");
        sb.AppendLine($"InputPort (16 bits): 0x{Convert.ToString(InputPort, 2).PadLeft(16, '0')}");
        sb.AppendLine($"ControllerInformationFlag: 0x{ControllerInformationFlag}");
        sb.AppendLine($"ControllerInformationFlag (16 bits): 0x{Convert.ToString(ControllerInformationFlag, 2).PadLeft(16, '0')}");
        sb.AppendLine($"CurrentPosition: {(CurrentPosition / 100.0):F2}");
        sb.AppendLine($"CurrentSpeed: {CurrentSpeed}");
        sb.AppendLine($"CurrentPushingForce: {CurrentPushingForce}");
        sb.AppendLine($"TargetPosition: {(TargetPosition / 100.0):F2}");
        sb.AppendLine($"Alarm1And2: {Alarm1And2}");
        sb.AppendLine($"Alarm3And4: {Alarm3And4}");
        return sb.ToString();
    }
}