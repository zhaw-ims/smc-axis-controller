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
}