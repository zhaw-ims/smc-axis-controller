using SMCAxisController.DataModel;

namespace SMCAxisController.Hardware;

public interface ISmcEthernetIpConnector
{
    void Connect();
    Task ReturnToOrigin();
    Task GoToPositionNumerical();
    void Disconnect();
    void GetData();
    void PowerOn();
    void PowerOff();
    void HoldOn();
    void HoldOff();
    Task Reset();
    Task AlarmReset();
    void ExitWaitingLoop();
    ControllerStatus Status { get; }
    MovementParameters MovementParameters { get; set; }
    ControllerOutputData ControllerOutputData { get; set; }
    ControllerInputData ControllerInputData { get; set; }
    ControllerProperties ControllerProperties { get; set; }
    event Func<Task> OnNewControllerData;
    event Action<string, MudBlazor.Severity> OnSnackBarMessage;
    
    
}