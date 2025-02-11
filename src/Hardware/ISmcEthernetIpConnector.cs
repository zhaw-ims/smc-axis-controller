using SMCAxisController.DataModel;

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
    void HoldOn();
    void HoldOff();
    void Reset();
    void AlarmReset();
    ControllerStatus Status { get; }
    MovementParameters MovementParameters { get; set; }
    ControllerOutputData ControllerOutputData { get; set; }
    ControllerInputData ControllerInputData { get; set; }
    ControllerProperties ControllerProperties { get; set; }
    event Action OnNewControllerData;
    
    
}