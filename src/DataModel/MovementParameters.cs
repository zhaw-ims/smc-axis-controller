using System.Text;

namespace SMCAxisController.DataModel;

public class MovementParameters
{ 
    public MovementMode MovementMode { get; set; }
    public int Speed { get; set; } = 20;
    public int TargetPosition { get; set; } = 400;
    public int Acceleration { get; set; } = 1000;
    public int Deceleration { get; set; } = 1000;
    public int PushingForce { get; set; } = 0;
    public int TriggerLv { get; set; } = 0;
    public int PushingSpeed { get; set; } = 0;
    public int PushingForceForPositioning { get; set; } = 100;
    public int Area1 { get; set; }
    public int Area2 { get; set; }
    public int PositioningWidth { get; set; } = 100;
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"MovementMode: {MovementMode}");
        sb.AppendLine($"Speed: {Speed}");
        sb.AppendLine($"TargetPosition: {TargetPosition / 100.0:F2}");
        sb.AppendLine($"Acceleration: {Acceleration}");
        sb.AppendLine($"Deceleration: {Deceleration}");
        sb.AppendLine($"PushingForce: {PushingForce}");
        sb.AppendLine($"TriggerLv: {TriggerLv}");
        sb.AppendLine($"PushingSpeed: {PushingSpeed}");
        sb.AppendLine($"PushingForceForPositioning: {PushingForceForPositioning}");
        sb.AppendLine($"Area1: {Area1 / 100.0:F2}");
        sb.AppendLine($"Area2: {Area2 / 100.0:F2}");
        sb.AppendLine($"PositioningWidth: {PositioningWidth / 100.0:F2}");
        return sb.ToString();
    }
}