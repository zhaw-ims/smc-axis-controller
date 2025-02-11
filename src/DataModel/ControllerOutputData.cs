using System.Text;

namespace SMCAxisController.DataModel;

public class ControllerOutputData
{
    
    // Output Word0
    public const UInt16 IN0 = 0x0001;
    public const UInt16 IN1 = 0x0002;
    public const UInt16 IN2 = 0x0004;
    public const UInt16 IN3 = 0x0008;
    public const UInt16 IN4 = 0x0010;
    public const UInt16 IN5 = 0x0020;
    
    public const UInt16 HOLD = 0x0100;
    public const UInt16 SVON = 0x0200;
    public const UInt16 DRIVE = 0x0400;
    public const UInt16 RESET = 0x0800;
    public const UInt16 SETUP = 0x1000;
    public const UInt16 JOG_MINUS = 0x2000;
    public const UInt16 JOG_PLUS = 0x4000;
    public const UInt16 FLGHT = 0x8000;
    
    // Output Word1
    public const UInt16 SPEED_RESTRICTION = 0x0002;
    public const UInt16 MOVEMENT_MODE = 0x0010;
    public const UInt16 SPEED = 0x0020;
    public const UInt16 POSITION = 0x0040;
    public const UInt16 ACCELERATION = 0x0080;
    public const UInt16 DECELLERATION = 0x0100;
    public const UInt16 PUSHING_FORCE = 0x0200;
    public const UInt16 TRIGGER_LV = 0x0400;
    public const UInt16 PUSHING_SPEED = 0x0800;
    public const UInt16 MOVING_FORCE = 0x1000;
    public const UInt16 AREA1 = 0x2000;
    public const UInt16 AREA2 = 0x4000;
    public const UInt16 IN_POSITION = 0x8000;
    public const UInt16 ALL_NUMERICAL_DATA = MOVEMENT_MODE | SPEED | POSITION | ACCELERATION | DECELLERATION
                                              | PUSHING_FORCE | TRIGGER_LV | PUSHING_SPEED | MOVING_FORCE | AREA1 | AREA2 | IN_POSITION;
    
    // Output Word2
    public const UInt16 START_FLAG = 0x0001;
    public const UInt16 MOVEMENT_MODE_ABS = 0x0100;
    public const UInt16 MOVEMENT_MODE_RELATIVE = 0x0200;
    
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
    public bool IsIn0(){
        return (OutputPortToWhichSignalsAreAllocated & IN0) != 0;
    }
    public bool IsIn1(){
        return (OutputPortToWhichSignalsAreAllocated & IN1) != 0;
    }
    public bool IsIn2(){
        return (OutputPortToWhichSignalsAreAllocated & IN2) != 0;
    }
    public bool IsIn3(){
        return (OutputPortToWhichSignalsAreAllocated & IN3) != 0;
    }
    public bool IsIn4(){
        return (OutputPortToWhichSignalsAreAllocated & IN4) != 0;
    }
    public bool IsIn5(){
        return (OutputPortToWhichSignalsAreAllocated & IN5) != 0;
    }
    
    public bool IsHold(){
        return (OutputPortToWhichSignalsAreAllocated & HOLD) != 0;
    }
    public bool IsSvon(){
        return (OutputPortToWhichSignalsAreAllocated & SVON) != 0;
    }
    public bool IsDrive(){
        return (OutputPortToWhichSignalsAreAllocated & DRIVE) != 0;
    }
    public bool IsReset(){
        return (OutputPortToWhichSignalsAreAllocated & RESET) != 0;
    }
    public bool IsSetup(){
        return (OutputPortToWhichSignalsAreAllocated & SETUP) != 0;
    }
    public bool IsJogMinus(){
        return (OutputPortToWhichSignalsAreAllocated & JOG_MINUS) != 0;
    }
    public bool IsJogPlus(){
        return (OutputPortToWhichSignalsAreAllocated & JOG_PLUS) != 0;
    }
    public bool IsFlght(){
        return (OutputPortToWhichSignalsAreAllocated & FLGHT) != 0;
    }
    
    // Word 2
    public bool IsStartFlag(){
        return (MovementModeAndStartFlag & START_FLAG) != 0;
    }
    public bool IsMovementModeAbs(){
        return (MovementModeAndStartFlag & MOVEMENT_MODE_ABS) != 0;
    }
    public bool IsMovementModeRelative(){
        return (MovementModeAndStartFlag & MOVEMENT_MODE_RELATIVE) != 0;
    }
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