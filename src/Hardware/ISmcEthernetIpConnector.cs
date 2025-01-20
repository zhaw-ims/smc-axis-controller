namespace SMCAxisController.Hardware;

public interface ISmcEthernetIpConnector
{
    void Connect();
    void MyTestFunction();
    void ReturnToOrigin();
    void GoToPositionNumerical();
    void Disconnect();
    void GetData();
    void PowerOn();
    void PowerOff();
    MovementMode MovementMode { get; set; }
    int Speed { get; set; }
    int TargetPosition { get; set; }
    int Acceleration { get; set; }
    int Deceleration { get; set; }
    int PushingForce { get; set; }
    int TriggerLv { get; set; }
    int PushingSpeed { get; set; }
    int PushingForceForPositioning { get; set; }
    int Area1 { get; set; }
    int Area2 { get; set; }
    int PositioningWidth { get; set; }
}