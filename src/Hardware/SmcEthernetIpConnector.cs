using Sres.Net.EEIP;

namespace SMCAxisController.Hardware;

public enum MovementMode
{
    Absolute,
    Relative
}
public enum StepData{
    MovementMode,
    Speed,
    TargetPosition,
    Acceleration,
    Deceleration,
    PushingForce,
    TriggerLv,
    PushingSpeed,
    PushingForceForPositioning,
    Area1,
    Area2,
    PositioningWidth,
}

public enum InputAreaMapping // p.32-36
{
    W0InputPortToWhichSignalsAreAllocated = 0,
    W1ControllerInformationFlag,
    W2CurrentPosition, //32
    W4CurrentSpeed,
    W5CurrentPushingForce,
    W6TargetPosition, //32
    W7Alarm1And2,
    W9Alarm3And4,
}

public enum OutputAreaMapping // p.37-40
{
    W0OutputPortToWhichSignalsAreAllocated,
    W1ControllingOfTheControllerAndNumericalDataFlag,
    W2MovementModeAndStartFlag,
    W3Speed,
    W4TargetPosition, // 32
    W6Acceleration,
    W7Deceleration,
    W8PushingForceThrustSettingValue,
    W9TriggerLv,
    W10PushingSpeed,
    W11MovingForce,
    W12Area1, // 32
    W14Area2, // 32
    W16InPosition, // 32,
}


public class SmcEthernetIpConnector : ISmcEthernetIpConnector 
{
    private readonly ILogger<SmcEthernetIpConnector> _logger;
    private EEIPClient _eeipClient = new EEIPClient();
    private string _ipAddress = "192.168.1.2";
    private const int _inputInstance = 100;
    private const int _outputInstance = 150;
    private const int _configInstance = 105; // not used
    
        // Input Word0
    private const UInt16 BUSY = 0x0100;
    private const UInt16 SVRE = 0x0200;
    private const UInt16 SETON = 0x0400;
    private const UInt16 INP = 0x0800;
    private const UInt16 AREA = 0x1000;
    private const UInt16 WAREA = 0x2000;
    private const UInt16 ESTOP = 0x4000;
    private const UInt16 ALARM = 0x8000;
    
    // Input Word1
    private const UInt16 READY = 0x0010;
    
    // Output Word0
    private const UInt16 IN0 = 0x0001;
    private const UInt16 IN1 = 0x0002;
    private const UInt16 IN2 = 0x0004;
    private const UInt16 IN3 = 0x0008;
    private const UInt16 IN4 = 0x0010;
    private const UInt16 IN5 = 0x0020;
    
    private const UInt16 HOLD = 0x0100;
    private const UInt16 SVON = 0x0200;
    private const UInt16 DRIVE = 0x0400;
    private const UInt16 RESET = 0x0800;
    private const UInt16 SETUP = 0x1000;
    private const UInt16 JOG_MINUS = 0x2000;
    private const UInt16 JOG_PLUS = 0x4000;
    private const UInt16 FLGHT = 0x8000;
    
    // Output Word1
    private const UInt16 SPEED_RESTRICTION = 0x0002;
    private const UInt16 MOVEMENT_MODE = 0x0010;
    private const UInt16 SPEED = 0x0020;
    private const UInt16 POSITION = 0x0040;
    private const UInt16 ACCELERATION = 0x0080;
    private const UInt16 DECELLERATION = 0x0100;
    private const UInt16 PUSHING_FORCE = 0x0200;
    private const UInt16 TRIGGER_LV = 0x0400;
    private const UInt16 PUSHING_SPEED = 0x0800;
    private const UInt16 MOVING_FORCE = 0x1000;
    private const UInt16 AREA1 = 0x2000;
    private const UInt16 AREA2 = 0x4000;
    private const UInt16 IN_POSITION = 0x8000;
    private const UInt16 ALL_NUMERICAL_DATA = MOVEMENT_MODE | SPEED | POSITION | ACCELERATION | DECELLERATION
        | PUSHING_FORCE | TRIGGER_LV | PUSHING_SPEED | MOVING_FORCE | AREA1 | AREA2 | IN_POSITION;
    
    // Output Word2
    private const UInt16 START_FLAG = 0x0001;
    private const UInt16 MOVEMENT_MODE_ABS = 0x0100;
    private const UInt16 MOVEMENT_MODE_RELATIVE = 0x0200;

    //TODO: Put to UI
    public MovementMode MovementMode { get; set; } = MovementMode.Absolute;
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
    
    public SmcEthernetIpConnector(ILogger<SmcEthernetIpConnector> logger)
    {
        _logger = logger;    
    }
    
    public static readonly Dictionary<StepData, (int Offset, int Size)> StepDataDictionary = new Dictionary<StepData, (int, int)>
    {
        { StepData.MovementMode, (0, 2) },
        { StepData.Speed, (2, 2) },
        { StepData.TargetPosition, (4, 4) },
        { StepData.Acceleration, (8, 2) },
        { StepData.Deceleration, (10, 2) },
        { StepData.PushingForce, (12, 2) },
        { StepData.TriggerLv, (14, 2) },
        { StepData.PushingSpeed, (16, 2) },
        { StepData.PushingForceForPositioning, (18, 2) },
        { StepData.Area1, (20, 4) },
        { StepData.Area2, (24, 4) },
        { StepData.PositioningWidth, (28, 4) }
    };
    
    public static readonly Dictionary<InputAreaMapping, (int Offset, int Size)> InputMemoryDictionary = new Dictionary<InputAreaMapping, (int, int)>
    {
        { InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, (0, 2) },
        { InputAreaMapping.W1ControllerInformationFlag, (2, 2) },
        { InputAreaMapping.W2CurrentPosition, (4, 4) },
        { InputAreaMapping.W4CurrentSpeed, (8, 2) },
        { InputAreaMapping.W5CurrentPushingForce, (10, 2) },
        { InputAreaMapping.W6TargetPosition, (12, 4) },
        { InputAreaMapping.W7Alarm1And2, (16, 2) },
        { InputAreaMapping.W9Alarm3And4, (18, 2) },
    };
    
    public static readonly Dictionary<OutputAreaMapping, (int Offset, int Size)> OutputMemoryDictionary = new Dictionary<OutputAreaMapping, (int, int)>
    {
        { OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, (0, 2) },
        { OutputAreaMapping.W1ControllingOfTheControllerAndNumericalDataFlag, (2, 2) },
        { OutputAreaMapping.W2MovementModeAndStartFlag, (4, 2) },
        { OutputAreaMapping.W3Speed, (6, 2) },
        { OutputAreaMapping.W4TargetPosition, (8, 4) }, // 32-bit
        { OutputAreaMapping.W6Acceleration, (12, 2) },
        { OutputAreaMapping.W7Deceleration, (14, 2) },
        { OutputAreaMapping.W8PushingForceThrustSettingValue, (16, 2) },
        { OutputAreaMapping.W9TriggerLv, (18, 2) },
        { OutputAreaMapping.W10PushingSpeed, (20, 2) },
        { OutputAreaMapping.W11MovingForce, (22, 2) },
        { OutputAreaMapping.W12Area1, (24, 4) }, // 32-bit
        { OutputAreaMapping.W14Area2, (28, 4) }, // 32-bit
        { OutputAreaMapping.W16InPosition, (32, 4) } // 32-bit
    };

    public int GetStepDataValue(byte[] data, StepData stepData)
    {
        var stepInfo = StepDataDictionary[stepData];

        // Validate data length
        if (data == null || data.Length < stepInfo.Offset + stepInfo.Size)
            throw new ArgumentException("Data array is too small or null for the requested StepData.");

        // Handle based on size
        return stepInfo.Size switch
        {
            2 => BitConverter.ToInt16(data, stepInfo.Offset), // 16-bit value
            4 => BitConverter.ToInt32(data, stepInfo.Offset), // 32-bit value
            _ => throw new NotSupportedException($"Unsupported size {stepInfo.Size} for StepData {stepData}")
        };
    }
    
    public int GetInputValue(byte[] data, InputAreaMapping inputAreaMapping)
    {
        var stepInfo = InputMemoryDictionary[inputAreaMapping];

        // Validate data length
        if (data == null || data.Length < stepInfo.Offset + stepInfo.Size)
            throw new ArgumentException("Data array is too small or null for the requested StepData.");

        // Handle based on size
        return stepInfo.Size switch
        {
            2 => BitConverter.ToInt16(data, stepInfo.Offset), // 16-bit value
            4 => BitConverter.ToInt32(data, stepInfo.Offset), // 32-bit value
            _ => throw new NotSupportedException($"Unsupported size {stepInfo.Size} for StepData {inputAreaMapping}")
        };
    }
    
    public int GetOutputValue(byte[] data, OutputAreaMapping outputAreaMapping)
    {
        var stepInfo = OutputMemoryDictionary[outputAreaMapping];

        // Validate data length
        if (data == null || data.Length < stepInfo.Offset + stepInfo.Size)
            throw new ArgumentException("Data array is too small or null for the requested StepData.");

        // Handle based on size
        return stepInfo.Size switch
        {
            2 => BitConverter.ToInt16(data, stepInfo.Offset), // 16-bit value
            4 => BitConverter.ToInt32(data, stepInfo.Offset), // 32-bit value
            _ => throw new NotSupportedException($"Unsupported size {stepInfo.Size} for StepData {outputAreaMapping}")
        };
    }
    
    public void SetOutputValue(byte[] data, OutputAreaMapping outputAreaMapping, int value)
    {
        var stepInfo = OutputMemoryDictionary[outputAreaMapping];

        // Validate data length
        if (data == null || data.Length < stepInfo.Offset + stepInfo.Size)
            throw new ArgumentException("Data array is too small or null for the requested OutputAreaMapping.");

        // Write data based on size
        switch (stepInfo.Size)
        {
            case 2: // 16-bit value
                Array.Copy(BitConverter.GetBytes((short)value), 0, data, stepInfo.Offset, 2);
                break;

            case 4: // 32-bit value
                Array.Copy(BitConverter.GetBytes(value), 0, data, stepInfo.Offset, 4);
                break;

            default:
                throw new NotSupportedException($"Unsupported size {stepInfo.Size} for OutputAreaMapping {outputAreaMapping}");
        }
    }
    
    public void SetOutputValue(OutputAreaMapping outputAreaMapping, int value)
    {
        // Retrieve the entire output data buffer
        var outputData = _eeipClient.AssemblyObject.getInstance(_outputInstance);

        // Modify the selected output value
        var stepInfo = OutputMemoryDictionary[outputAreaMapping];

        // Validate data length
        if (outputData == null || outputData.Length < stepInfo.Offset + stepInfo.Size)
            throw new ArgumentException("Output data array is too small for the requested OutputAreaMapping.");

        // Write the value based on size
        switch (stepInfo.Size)
        {
            case 2: // 16-bit value
                Array.Copy(BitConverter.GetBytes((short)value), 0, outputData, stepInfo.Offset, 2);
                break;

            case 4: // 32-bit value
                Array.Copy(BitConverter.GetBytes(value), 0, outputData, stepInfo.Offset, 4);
                break;

            default:
                throw new NotSupportedException($"Unsupported size {stepInfo.Size} for OutputAreaMapping {outputAreaMapping}");
        }

        // Send the modified output data back to the device
        _eeipClient.AssemblyObject.setInstance(_outputInstance, outputData);

        Console.WriteLine($"Set {outputAreaMapping} to {value} and updated all outputs.");
    }

    void GetAllOutputs()
    {
        var outputData = _eeipClient.AssemblyObject.getInstance(_outputInstance);
        
        try
        {
            int outputPortToWhichSignalsAreAllocated = GetOutputValue(outputData, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated);
            int controllingOfTheControllerAndNumericalDataFlag = GetOutputValue(outputData, OutputAreaMapping.W1ControllingOfTheControllerAndNumericalDataFlag);
            int movementModeAndStartFlag = GetOutputValue(outputData, OutputAreaMapping.W2MovementModeAndStartFlag);
            int speed = GetOutputValue(outputData, OutputAreaMapping.W3Speed);
            int targetPosition = GetOutputValue(outputData, OutputAreaMapping.W4TargetPosition);
            int acceleration = GetOutputValue(outputData, OutputAreaMapping.W6Acceleration);
            int deceleration = GetOutputValue(outputData, OutputAreaMapping.W7Deceleration);
            int pushingForceThrustSettingValue = GetOutputValue(outputData, OutputAreaMapping.W8PushingForceThrustSettingValue);
            int triggerLv = GetOutputValue(outputData, OutputAreaMapping.W9TriggerLv);
            int pushingSpeed = GetOutputValue(outputData, OutputAreaMapping.W10PushingSpeed);
            int pushingForce = GetOutputValue(outputData, OutputAreaMapping.W11MovingForce);
            int area1 = GetOutputValue(outputData, OutputAreaMapping.W12Area1);
            int area2 = GetOutputValue(outputData, OutputAreaMapping.W14Area2);
            int  inPosition = GetOutputValue(outputData, OutputAreaMapping.W16InPosition);
            
            Console.WriteLine($"\nOutputs:");
            Console.WriteLine($"W0OutputPortToWhichSignalsAreAllocated: {outputPortToWhichSignalsAreAllocated}");
            Console.WriteLine($"W0OutputPortToWhichSignalsAreAllocated (16 bits): 0x{Convert.ToString(outputPortToWhichSignalsAreAllocated, 2).PadLeft(16, '0')}");
            Console.WriteLine($"W1ControllingOfTheControllerAndNumericalDataFlag: {controllingOfTheControllerAndNumericalDataFlag}");
            Console.WriteLine($"W1ControllingOfTheControllerAndNumericalDataFlag (16 bits): 0x{Convert.ToString(controllingOfTheControllerAndNumericalDataFlag, 2).PadLeft(16, '0')}");
            Console.WriteLine($"W2MovementModeAndStartFlag: {movementModeAndStartFlag}");
            Console.WriteLine($"W2MovementModeAndStartFlag (16 bits): 0x{Convert.ToString(movementModeAndStartFlag, 2).PadLeft(16, '0')}");
            Console.WriteLine($"W3Speed: {speed}");
            Console.WriteLine($"W4TargetPosition: {targetPosition/100:F2}");
            Console.WriteLine($"W6Acceleration: {acceleration}");
            Console.WriteLine($"W7Deceleration: {deceleration}");
            Console.WriteLine($"W8PushingForceThrustSettingValue: {pushingForceThrustSettingValue}");
            Console.WriteLine($"W9TriggerLv: {triggerLv}");
            Console.WriteLine($"W10PushingSpeed: {pushingSpeed}");
            Console.WriteLine($"PushingForce: {pushingForce}");
            Console.WriteLine($"W12Area1: {area1/100:F2}");
            Console.WriteLine($"W14Area2: {area2/100:F2}");
            Console.WriteLine($"W16InPosition: {inPosition/100:F2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    void GetAllInputs()
    {
        var inputData = _eeipClient.AssemblyObject.getInstance(_inputInstance);
        try
        {
            int inputPort = GetInputValue(inputData, InputAreaMapping.W0InputPortToWhichSignalsAreAllocated);
            int controllerInformationFlag = GetInputValue(inputData, InputAreaMapping.W1ControllerInformationFlag);
            int currentPosition = GetInputValue(inputData, InputAreaMapping.W2CurrentPosition);
            int currentSpeed = GetInputValue(inputData, InputAreaMapping.W4CurrentSpeed);
            int currentPushingForce = GetInputValue(inputData, InputAreaMapping.W5CurrentPushingForce);
            int  targetPosition = GetInputValue(inputData, InputAreaMapping.W6TargetPosition);
            int alarm1And2 = GetInputValue(inputData, InputAreaMapping.W7Alarm1And2);
            int alarm3And4 = GetInputValue(inputData, InputAreaMapping.W9Alarm3And4);
            
            Console.WriteLine($"\nInputs:");
            Console.WriteLine($"InputPort: {inputPort}");
            Console.WriteLine($"InputPort (16 bits): 0x{Convert.ToString(inputPort, 2).PadLeft(16, '0')}");
            Console.WriteLine($"W1ControllerInformationFlag: 0x{controllerInformationFlag}");
            Console.WriteLine($"W1ControllerInformationFlag (16 bits): 0x{Convert.ToString(controllerInformationFlag, 2).PadLeft(16, '0')}");
            Console.WriteLine($"W2CurrentPosition: {currentPosition/100:F2}");
            Console.WriteLine($"W4CurrentSpeed: {currentSpeed}");
            Console.WriteLine($"W5CurrentPushingForce: {currentPushingForce}");
            Console.WriteLine($"W4TargetPosition: {targetPosition/100:F2}");
            Console.WriteLine($"W7Alarm1And2: {alarm1And2}");
            Console.WriteLine($"W9Alarm3And4: {alarm3And4}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    void GetStepData(int stepNumber)
    {
        var stepData = _eeipClient.GetAttributeSingle(0x67, stepNumber, 100);
        try
        {
            int movementMode = GetStepDataValue(stepData, StepData.MovementMode);
            int speed = GetStepDataValue(stepData, StepData.Speed);
            int targetPosition = GetStepDataValue(stepData, StepData.TargetPosition);
            int acceleration = GetStepDataValue(stepData, StepData.Acceleration);
            int deceleration = GetStepDataValue(stepData, StepData.Deceleration);
            int pushingForce = GetStepDataValue(stepData, StepData.PushingForce);
            int triggerLv = GetStepDataValue(stepData, StepData.TriggerLv);
            int pushingSpeed = GetStepDataValue(stepData, StepData.PushingSpeed);
            int pushingForceForPositioning = GetStepDataValue(stepData, StepData.PushingForceForPositioning);
            int area1 = GetStepDataValue(stepData, StepData.Area1);
            int area2 = GetStepDataValue(stepData, StepData.Area2);
            int positioningWidth = GetStepDataValue(stepData, StepData.PositioningWidth);
            
            Console.WriteLine($"\nStep Data: {stepNumber}");
            Console.WriteLine($"MovementMode: {movementMode}");
            Console.WriteLine($"W3Speed: {speed}");
            Console.WriteLine($"W4TargetPosition: {targetPosition/100:F2}");
            Console.WriteLine($"W6Acceleration: {acceleration}");
            Console.WriteLine($"W7Deceleration: {deceleration}");
            Console.WriteLine($"PushingForce: {pushingForce}");
            Console.WriteLine($"W9TriggerLv: {triggerLv}");
            Console.WriteLine($"W10PushingSpeed: {pushingSpeed}");
            Console.WriteLine($"PushingForceForPositioning: {pushingForceForPositioning}");
            Console.WriteLine($"W12Area1: {area1/100:F2}");
            Console.WriteLine($"W14Area2: {area2/100:F2}");
            Console.WriteLine($"PositioningWidth: {positioningWidth/100:F2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    public void SetStepDataValue(StepData outputMemory, int value, byte[] stepData)
    {
        // Modify the selected output value
        var stepInfo = StepDataDictionary[outputMemory];

        // Validate data length
        if (stepData == null || stepData.Length < stepInfo.Offset + stepInfo.Size)
            throw new ArgumentException("Output data array is too small for the requested OutputAreaMapping.");

        // Write the value based on size
        switch (stepInfo.Size)
        {
            case 2: // 16-bit value
                Array.Copy(BitConverter.GetBytes((short)value), 0, stepData, stepInfo.Offset, 2);
                break;

            case 4: // 32-bit value
                Array.Copy(BitConverter.GetBytes(value), 0, stepData, stepInfo.Offset, 4);
                break;

            default:
                throw new NotSupportedException($"Unsupported size {stepInfo.Size} for OutputAreaMapping {outputMemory}");
        }

        // Send the modified output data back to the device
        

        Console.WriteLine($"Set {outputMemory} to {value} and updated all outputs.");
    }
    
    void SetStepData(int stepNumber)
    {
        byte[] stepData = new byte[32];

        SetStepDataValue(StepData.MovementMode, 1, stepData);
        SetStepDataValue(StepData.Speed, 30, stepData);
        SetStepDataValue(StepData.TargetPosition, 400, stepData);
        SetStepDataValue(StepData.Acceleration, 1000, stepData);
        SetStepDataValue(StepData.Deceleration, 1000, stepData);
        SetStepDataValue(StepData.PushingForce, 0, stepData);
        SetStepDataValue(StepData.TriggerLv, 0, stepData);
        SetStepDataValue(StepData.PushingSpeed, 0, stepData);
        SetStepDataValue(StepData.PushingForceForPositioning, 100, stepData);
        SetStepDataValue(StepData.Area1, 0, stepData);
        SetStepDataValue(StepData.Area2, 0, stepData);
        SetStepDataValue(StepData.PositioningWidth, 50, stepData);
        
        
        _eeipClient.SetAttributeSingle(0x67, stepNumber, 100, stepData);
    }

    public void Connect()
    {
        // use the Standard Port for Ethernet/IP TCP-connections 0xAF12
        _eeipClient.RegisterSession(_ipAddress);
    }

    public void GetData()
    {
        GetAllInputs();
        GetAllOutputs();
    }

    public void MyTestFunction()
    {
        // UInt16 outPortValue = 0;
        // ControlWordHandler.SetStepNumber(ref outPortValue, 1);
        // ControlWordHandler.SetControlWordBit(ref outPortValue, ControlWordBits.SVON, false);
        
        //     SetOutputValue(OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, outPortValue);
        //     SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag, 11);
        
        // PowerOn();
        // ReturnToOrigin();
        // GoToPositionNumerical();
    }

    public void ReturnToOrigin()
    {
        SetOutputValue(OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, SVON | SETUP);
        //WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, BUSY, true); // not neccessary to check
        WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, INP, true);
    }

    public void PowerOn()
    {
        SetOutputValue(OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, SVON); 
    }
    
    public void PowerOff()
    {
        SetOutputValue(OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, 0); 
    }
    public void GoToPositionNumerical()
    {
        try
        {
            // (1)
            // Numerical operation p.57
            // Confirm that Word2, bit0: Start flag = OFF. Input Word2, bit0: Start flag = OFF when it is ON.
            if(MovementMode == MovementMode.Absolute)
            {
                SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag,
                    MOVEMENT_MODE_ABS); // startflag off
            }
            else
            {
                SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag,
                    MOVEMENT_MODE_RELATIVE); // startflag off
            }
            // (2)
            // Input the step data No. to be specified by Word0, bit0-5:IN0-5 E.g.)
            // Specify step data No.1→bit0:IN0 = ON bit1-5:IN1-5 = OFF This is the Base step No that will be used.
            SetOutputValue(OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, SVON | IN0); // step data No.1 
            
            // (3)
            // Specify the numerical operation input flags which control the numerical operation data to be entered, by Word1, bit4-15.
            // Turn ON the relevant flag which must be numerically input into the specified step data and turn OFF the relevant flag which is not required.
            // E.g.) Only [position] of the numerical operation data input flag must be specified. → Word1, bit6=ON, Word1, bit4-5,7-15=OFF.
            SetOutputValue(OutputAreaMapping.W1ControllingOfTheControllerAndNumericalDataFlag, ALL_NUMERICAL_DATA); // all
            
            // (4)
            // Input Word2, bit8-9:Movement mode and Word3-17:Numerical operation data.
            // E.g.) Input [Position] 50.00 [mm]. 5000[0.01mm]=(00001388)h→
            // Word4: Target position(L) = (1388)h
            // Word5: Target position (H) = (0000)h
            
            if(MovementMode == MovementMode.Absolute)
            {
                SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag,
                    MOVEMENT_MODE_ABS); // absolute, startflag off
            }
            else
            {
                SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag,
                    MOVEMENT_MODE_RELATIVE); // relative, startflag off
            }
            
            SetOutputValue(OutputAreaMapping.W3Speed, Speed);
            SetOutputValue(OutputAreaMapping.W4TargetPosition, TargetPosition);
            SetOutputValue(OutputAreaMapping.W6Acceleration, Acceleration);
            SetOutputValue(OutputAreaMapping.W7Deceleration, Deceleration);
            SetOutputValue(OutputAreaMapping.W11MovingForce, PushingForce);
            SetOutputValue(OutputAreaMapping.W9TriggerLv, TriggerLv);
            SetOutputValue(OutputAreaMapping.W10PushingSpeed, PushingSpeed);
            SetOutputValue(OutputAreaMapping.W11MovingForce, PushingForceForPositioning);
            SetOutputValue(OutputAreaMapping.W12Area1, Area1);
            SetOutputValue(OutputAreaMapping.W14Area2, Area2);
            SetOutputValue(OutputAreaMapping.W16InPosition, PositioningWidth);  
            
            // (5)
            // Input the numerical operation data input flag bit and numerical operation data, and then input Word2,
            // bit0: Start flag = ON. The numerical operation data will be transmitted when the start flag is ON.
            //SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag, START_FLAG | MOVEMENT_MODE_ABS); // absolute, set start flag
            if(MovementMode == MovementMode.Absolute)
            {
                SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag,
                    START_FLAG | MOVEMENT_MODE_ABS); // absolute, startflag off
            }
            else
            {
                SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag,
                    START_FLAG | MOVEMENT_MODE_RELATIVE); // relative, startflag off
            }
            
            //(6)
            // When the actuator starts operating, Word0, bit8: BUSY = ON will be output. Then, input Word2, bit0: Start flag = OFF.
            WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, BUSY, true);
            if(MovementMode == MovementMode.Absolute)
            {
                SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag,
                    MOVEMENT_MODE_ABS); // startflag off
            }
            else
            {
                SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag,
                    MOVEMENT_MODE_RELATIVE); // startflag off
            }
            
            // (7) When the actuator reached the target position, Word0, bit11: INP=ON is output.
            // (Refer to "INP" section (P.34) for signal ON conditions) When the actuator stops, Word0, bit8: BUSY=OFF will be output.
            // The completion of the actuator operation is validated when both Word0, bit11: INP=ON and Word0, bit8: BUSY=OFF are established.
            WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, INP, true); // INP
            WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, BUSY, false); // BUSY
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    void WaitForFlag(InputAreaMapping input, UInt16 bitMask, bool awaitedValue)
    {
        //TODO: Replace while(true)
        while (true)
        {
            var inputData = _eeipClient.AssemblyObject.getInstance(_inputInstance);
            try
            {
                int inputPort = GetInputValue(inputData, InputAreaMapping.W0InputPortToWhichSignalsAreAllocated);
                var val = inputPort & bitMask;
                bool isConditionMet = (val > 0);
                if (isConditionMet == awaitedValue)
                    return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Thread.Sleep(10);
        }

    }
    public void Disconnect()
    {
        _eeipClient.UnRegisterSession();
    }
}