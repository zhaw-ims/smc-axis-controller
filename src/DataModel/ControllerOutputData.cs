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
}