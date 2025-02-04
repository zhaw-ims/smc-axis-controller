using System.Text;

namespace SMCAxisController.DataModel;

public class ControllerOutputData
{
    public int OutputPortToWhichSignalsAreAllocated { get; set; }
    public int ControllingOfTheControllerAndNumericalDataFlag { get; set; }
    public int MovementModeAndStartFlag { get; set; }
    public int Speed { get; set; }
    public int TargetPosition { get; set; }
    public int Acceleration { get; set; }
    public int Deceleration { get; set; }
    public int PushingForceThrustSettingValue { get; set; }
    public int TriggerLv { get; set; }
    public int PushingSpeed { get; set; }
    public int PushingForce { get; set; }
    public int Area1 { get; set; }
    public int Area2 { get; set; }
    public int InPosition { get; set; } 
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Outputs:");
        sb.AppendLine($"W0OutputPortToWhichSignalsAreAllocated: {OutputPortToWhichSignalsAreAllocated}");
        sb.AppendLine($"W0OutputPortToWhichSignalsAreAllocated (16 bits): 0x{Convert.ToString(OutputPortToWhichSignalsAreAllocated, 2).PadLeft(16, '0')}");
        sb.AppendLine($"W1ControllingOfTheControllerAndNumericalDataFlag: {ControllingOfTheControllerAndNumericalDataFlag}");
        sb.AppendLine($"W1ControllingOfTheControllerAndNumericalDataFlag (16 bits): 0x{Convert.ToString(ControllingOfTheControllerAndNumericalDataFlag, 2).PadLeft(16, '0')}");
        sb.AppendLine($"W2MovementModeAndStartFlag: {MovementModeAndStartFlag}");
        sb.AppendLine($"W2MovementModeAndStartFlag (16 bits): 0x{Convert.ToString(MovementModeAndStartFlag, 2).PadLeft(16, '0')}");
        sb.AppendLine($"W3Speed: {Speed}");
        sb.AppendLine($"W4TargetPosition: {(TargetPosition / 100.0):F2}");
        sb.AppendLine($"W6Acceleration: {Acceleration}");
        sb.AppendLine($"W7Deceleration: {Deceleration}");
        sb.AppendLine($"W8PushingForceThrustSettingValue: {PushingForceThrustSettingValue}");
        sb.AppendLine($"W9TriggerLv: {TriggerLv}");
        sb.AppendLine($"W10PushingSpeed: {PushingSpeed}");
        sb.AppendLine($"PushingForce: {PushingForce}");
        sb.AppendLine($"W12Area1: {(Area1 / 100.0):F2}");
        sb.AppendLine($"W14Area2: {(Area2 / 100.0):F2}");
        sb.AppendLine($"W16InPosition: {(InPosition / 100.0):F2}");
        return sb.ToString();
    }
}