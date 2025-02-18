using System.Text;
using Sres.Net.EEIP;

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
    
    public void SetInNumber(int number)
    {
        ClearInNumber(); 
        OutputPortToWhichSignalsAreAllocated |= (number & 0x003F); // set new value
    }
    public void ClearInNumber()
    {
        OutputPortToWhichSignalsAreAllocated &= ~0x003F; // Clear lowest 6 bits    
    }
    public void SetNumericalDataFlags(UInt16 flags)
    {
        ClearNumericalDataFlags();
        ControllingOfTheControllerAndNumericalDataFlag |= flags;    
    }
    public void ClearNumericalDataFlags(UInt16 flags)
    {
        ControllingOfTheControllerAndNumericalDataFlag &= ~flags;
    }
    public void ClearNumericalDataFlags()
    {
        ControllingOfTheControllerAndNumericalDataFlag = 0;
    }
    
    public void SetIn0() => OutputPortToWhichSignalsAreAllocated |= IN0;
    public void ClearIn0() => OutputPortToWhichSignalsAreAllocated &= ~IN0;

    public void SetIn1() => OutputPortToWhichSignalsAreAllocated |= IN1;
    public void ClearIn1() => OutputPortToWhichSignalsAreAllocated &= ~IN1;

    public void SetIn2() => OutputPortToWhichSignalsAreAllocated |= IN2;
    public void ClearIn2() => OutputPortToWhichSignalsAreAllocated &= ~IN2;

    public void SetIn3() => OutputPortToWhichSignalsAreAllocated |= IN3;
    public void ClearIn3() => OutputPortToWhichSignalsAreAllocated &= ~IN3;

    public void SetIn4() => OutputPortToWhichSignalsAreAllocated |= IN4;
    public void ClearIn4() => OutputPortToWhichSignalsAreAllocated &= ~IN4;

    public void SetIn5() => OutputPortToWhichSignalsAreAllocated |= IN5;
    public void ClearIn5() => OutputPortToWhichSignalsAreAllocated &= ~IN5;

    // Control signals
    public void SetHold() => OutputPortToWhichSignalsAreAllocated |= HOLD;
    public void ClearHold() => OutputPortToWhichSignalsAreAllocated &= ~HOLD;

    public void SetSvon() => OutputPortToWhichSignalsAreAllocated |= SVON;
    public void ClearSvon() => OutputPortToWhichSignalsAreAllocated &= ~SVON;

    public void SetDrive() => OutputPortToWhichSignalsAreAllocated |= DRIVE;
    public void ClearDrive() => OutputPortToWhichSignalsAreAllocated &= ~DRIVE;

    public void SetReset() => OutputPortToWhichSignalsAreAllocated |= RESET;
    public void ClearReset() => OutputPortToWhichSignalsAreAllocated &= ~RESET;

    public void SetSetup() => OutputPortToWhichSignalsAreAllocated |= SETUP;
    public void ClearSetup() => OutputPortToWhichSignalsAreAllocated &= ~SETUP;

    public void SetJogMinus() => OutputPortToWhichSignalsAreAllocated |= JOG_MINUS;
    public void ClearJogMinus() => OutputPortToWhichSignalsAreAllocated &= ~JOG_MINUS;

    public void SetJogPlus() => OutputPortToWhichSignalsAreAllocated |= JOG_PLUS;
    public void ClearJogPlus() => OutputPortToWhichSignalsAreAllocated &= ~JOG_PLUS;

    public void SetFlght() => OutputPortToWhichSignalsAreAllocated |= FLGHT;
    public void ClearFlght() => OutputPortToWhichSignalsAreAllocated &= ~FLGHT;

    // Word 2
    public void SetStartFlag() => MovementModeAndStartFlag |= START_FLAG;
    public void ClearStartFlag() => MovementModeAndStartFlag &= ~START_FLAG;

    public void SetMovementModeAbs()
    {
        ClearMovementModeRelative(); // Ensure relative mode is disabled
        MovementModeAndStartFlag |= MOVEMENT_MODE_ABS; // Set absolute mode
    }
    public void ClearMovementModeAbs() => MovementModeAndStartFlag &= ~MOVEMENT_MODE_ABS;

    public void SetMovementModeRelative()
    {
        ClearMovementModeAbs(); // Ensure absolute mode is disabled
        MovementModeAndStartFlag |= MOVEMENT_MODE_RELATIVE; // Set relative mode
    }
    public void ClearMovementModeRelative() => MovementModeAndStartFlag &= ~MOVEMENT_MODE_RELATIVE;
    
    // Set and send functions for in-number
    public void SetInNumberAndSend(EEIPClient eeipClient, int number)
    {
        ClearInNumber();
        OutputPortToWhichSignalsAreAllocated |= (number & 0x003F);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void ClearInNumberAndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated &= ~0x003F;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    // Set and send functions for numerical data flags
    public void SetNumericalDataFlagsAndSend(EEIPClient eeipClient, UInt16 flags)
    {
        ClearNumericalDataFlags();
        ControllingOfTheControllerAndNumericalDataFlag |= flags;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W1ControllingOfTheControllerAndNumericalDataFlag, ControllingOfTheControllerAndNumericalDataFlag);
    }

    public void ClearNumericalDataFlagsAndSend(EEIPClient eeipClient, UInt16 flags)
    {
        ControllingOfTheControllerAndNumericalDataFlag &= ~flags;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W1ControllingOfTheControllerAndNumericalDataFlag, ControllingOfTheControllerAndNumericalDataFlag);
    }

    public void ClearAllNumericalDataFlagsAndSend(EEIPClient eeipClient)
    {
        ControllingOfTheControllerAndNumericalDataFlag = 0;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W1ControllingOfTheControllerAndNumericalDataFlag, ControllingOfTheControllerAndNumericalDataFlag);
    }

    // Input signals
    public void SetIn0AndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated |= IN0;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void ClearIn0AndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated &= ~IN0;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void SetIn1AndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated |= IN1;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void ClearIn1AndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated &= ~IN1;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void SetIn2AndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated |= IN2;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void ClearIn2AndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated &= ~IN2;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    // Control signals
    public void SetHoldAndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated |= HOLD;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void ClearHoldAndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated &= ~HOLD;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void SetSvonAndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated |= SVON;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }

    public void ClearSvonAndSend(EEIPClient eeipClient)
    {
        OutputPortToWhichSignalsAreAllocated &= ~SVON;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }
    
    public void SetResetAndSend(EEIPClient eeipClient)
    { 
        OutputPortToWhichSignalsAreAllocated |= RESET;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }
    public void ClearResetAndSend(EEIPClient eeipClient)
    { 
        OutputPortToWhichSignalsAreAllocated &= ~RESET;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }
    public void SetSetupAndSend(EEIPClient eeipClient)
    { 
        OutputPortToWhichSignalsAreAllocated |= SETUP;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }
    public void ClearSetupAndSend(EEIPClient eeipClient)
    { 
        OutputPortToWhichSignalsAreAllocated &= ~SETUP;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, OutputPortToWhichSignalsAreAllocated);
    }
    

    // Word 2 settings
    public void SetStartFlagAndSend(EEIPClient eeipClient)
    {
        MovementModeAndStartFlag |= START_FLAG;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag, MovementModeAndStartFlag);
    }
    public void ClearStartFlagAndSend(EEIPClient eeipClient)
    {
        MovementModeAndStartFlag &= ~START_FLAG;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag, MovementModeAndStartFlag);
    }

    public void SetMovementModeAbsAndSend(EEIPClient eeipClient)
    {
        ClearMovementModeRelative();
        MovementModeAndStartFlag |= MOVEMENT_MODE_ABS;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag, MovementModeAndStartFlag);
    }

    public void SetMovementModeRelativeAndSend(EEIPClient eeipClient)
    {
        ClearMovementModeAbs();
        MovementModeAndStartFlag |= MOVEMENT_MODE_RELATIVE;
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag, MovementModeAndStartFlag);
    }

    // Input signals
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

    public void SendMovementParameters(EEIPClient eeipClient, MovementParameters movementParameters)
    {
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W3Speed, movementParameters.Speed);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W4TargetPosition, movementParameters.TargetPosition);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W6Acceleration, movementParameters.Acceleration);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W7Deceleration, movementParameters.Deceleration);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W11MovingForce, movementParameters.PushingForce);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W9TriggerLv, movementParameters.TriggerLv);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W10PushingSpeed, movementParameters.PushingSpeed);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W11MovingForce, movementParameters.PushingForceForPositioning);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W12Area1, movementParameters.Area1);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W14Area2, movementParameters.Area2);
        SmcOutputHelper.SetOutputValue(eeipClient, OutputAreaMapping.W16InPosition, movementParameters.PositioningWidth);
    }
}