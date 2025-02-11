using System.Text;

namespace SMCAxisController.DataModel;

public class ControllerInputData
{
    // Input Word0
    public const UInt16 OUT0 = 0x0001;
    public const UInt16 OUT1 = 0x0002;
    public const UInt16 OUT2 = 0x0004;
    public const UInt16 OUT3 = 0x0008;
    public const UInt16 OUT4 = 0x0010;
    public const UInt16 OUT5 = 0x0020;
    
    public const UInt16 BUSY = 0x0100;
    public const UInt16 SVRE = 0x0200;
    public const UInt16 SETON = 0x0400;
    public const UInt16 INP = 0x0800;
    public const UInt16 AREA = 0x1000;
    public const UInt16 WAREA = 0x2000;
    public const UInt16 ESTOP = 0x4000;
    public const UInt16 ALARM = 0x8000;
    
    // Input Word1
    private const UInt16 READY = 0x0010;
    
    public int InputPort { get; set; }
    public int ControllerInformationFlag { get; set; }
    public int CurrentPosition { get; set; } = 1;
    public int CurrentSpeed { get; set; }
    public int CurrentPushingForce { get; set; }
    public int TargetPosition { get; set; }
    public int Alarm1And2 { get; set; }
    public int Alarm3And4 { get; set; }
    
    // Word 0
    public bool IsOut0(){
        return (InputPort & OUT0) != 0;
    }
    public bool IsOut1(){
        return (InputPort & OUT1) != 0;
    }
    public bool IsOut2(){
        return (InputPort & OUT2) != 0;
    }
    public bool IsOut3(){
        return (InputPort & OUT3) != 0;
    }
    public bool IsOut4(){
        return (InputPort & OUT4) != 0;
    }
    public bool IsOut5(){
        return (InputPort & OUT5) != 0;
    }
    public bool IsBusy(){
        return (InputPort & BUSY) != 0;
    }
    public bool IsSvre(){
        return (InputPort & SVRE) != 0;
    }
    public bool IsSeton(){
        return (InputPort & SETON) != 0;
    }
    public bool IsInp(){
        return (InputPort & INP) != 0;
    }
    public bool IsArea(){
        return (InputPort & AREA) != 0;
    }
    public bool IsWarea(){
        return (InputPort & WAREA) != 0;
    }
    public bool IsEstop(){
        return (InputPort & ESTOP) != 0;
    }
    public bool IsAlarm(){
        var ret = (InputPort & ALARM) != 0; 
        return (InputPort & ALARM) != 0;
    }
    
    public bool IsReady(){
        return (ControllerInformationFlag & READY) != 0;
    }
    
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