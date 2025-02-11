using Sres.Net.EEIP;
using SMCAxisController.DataModel;

namespace SMCAxisController.Hardware;

public class SmcEthernetIpConnector : ISmcEthernetIpConnector 
{
    private readonly ILogger<SmcEthernetIpConnector> _logger;
    private EEIPClient _eeipClient = new EEIPClient();
    private const int _inputInstance = 100;
    private const int _outputInstance = 150;
    private const int _configInstance = 105; // not used
    
    public ControllerProperties ControllerProperties { get; set; } = new ControllerProperties();
    public MovementParameters MovementParameters { get; set; } = new MovementParameters();
    public ControllerOutputData ControllerOutputData { get; set; } = new ControllerOutputData();
    public ControllerInputData ControllerInputData { get; set; } = new ControllerInputData();
    public ControllerStatus Status { get; private set; } = ControllerStatus.NotConnected;
    
    public event Action OnNewControllerData;
    
    private void NotifyNewControllerData() => OnNewControllerData?.Invoke();

    //TODO: Put to UI
    public MovementMode MovementMode { get; set; } = MovementMode.Absolute;
    
    public SmcEthernetIpConnector(ILogger<SmcEthernetIpConnector> logger)
    {
        _logger = logger;    
    }
    
    public void Connect()
    {
        try
        {
            // use the Standard Port for Ethernet/IP TCP-connections 0xAF12
            Status = ControllerStatus.Connecting;
            NotifyNewControllerData();
            _eeipClient.RegisterSession(ControllerProperties.Ip);
            Status = ControllerStatus.Connected;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error when connecting to: {ControllerProperties.Name} {ex.Message}");
            Status = ControllerStatus.NotConnected;
        }
        NotifyNewControllerData();
    }
    public void GetData()
    {
        try
        {
            ControllerInputData = SmcInputHelper.GetAllInputs(_eeipClient);
            _logger.LogDebug(ControllerInputData.ToString());
            ControllerOutputData = SmcOutputHelper.GetAllOutputs(_eeipClient);
            _logger.LogDebug(ControllerOutputData.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Error: {ex.Message}");
        }
        NotifyNewControllerData();
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
        SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.OutputPortToWhichSignalsAreAllocated | ControllerOutputData.SVON | ControllerOutputData.SETUP);
        //WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, BUSY, true); // not neccessary to check
        WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, ControllerInputData.INP, true);
    }

    public void PowerOn()
    {
        SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.OutputPortToWhichSignalsAreAllocated | ControllerOutputData.SVON); 
    }
    
    public void PowerOff()
    {
        //SetOutputValue(OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, 0);
        SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.OutputPortToWhichSignalsAreAllocated & (~ControllerOutputData.SVON));
    }

    public void Reset() // [5]
    { 
        // (1)
        // During operation (“BUSY” is ON) “RESET" is turned ON
        SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.OutputPortToWhichSignalsAreAllocated | ControllerOutputData.RESET); 
        
        // (2)
        // “BUSY” and “OUT0” to “OUT5” are OFF.
        
        // (3)
        // The actuator decelerates to stop (controlled).    
    }
    
    public void HoldOn()
    {
        // (1)
        // During operation ("BUSY" is ON), turn ON "HOLD".
        SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.OutputPortToWhichSignalsAreAllocated | ControllerOutputData.HOLD); 
        
        // (2)
        // "BUSY" turns OFF. (The actuator stops.)
        
        // (3)
        // Turn OFF "HOLD".
        
        // (4)
        // "BUSY" turns ON. (The actuator restarts.)    
    }
    
    public void HoldOff()
    {
        // (1)
        // During operation ("BUSY" is ON), turn ON "HOLD".
        
        // (2)
        // "BUSY" turns OFF. (The actuator stops.)
        
        // (3)
        // Turn OFF "HOLD".
        SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.OutputPortToWhichSignalsAreAllocated & (~ControllerOutputData.HOLD)); 
        
        // (4)
        // "BUSY" turns ON. (The actuator restarts.)    
    }

    public void AlarmReset()
    {
        // (1)
        // Alarm generated “ALARM” turns ON.
        // Alarm group is output to “OUT0” to “OUT3”.
        // Alarm code is output.
        // For memory to be checked and detailed,
        // Please refer to 9. Memory map (P.32)
        // 15.1 Alarm group signals (P.62)
        // 15.3 Alarms and countermeasures (P.63)
        
        //(2)
        // Turn ON "RESET".
        SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.OutputPortToWhichSignalsAreAllocated | ControllerOutputData.RESET); 
        
        // (3)
        // "ALARM" turns OFF, “OUT0” to “OUT3” turn OFF. (The alarm is deactivated.)    
    }
    public void GoToPositionNumerical() // Numerical operation p.57
    {
        try
        {
            // (1)
            // Confirm that Word2, bit0: Start flag = OFF. Input Word2, bit0: Start flag = OFF when it is ON.
            if(MovementMode == MovementMode.Absolute)
            {
                SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag,
                    ControllerOutputData.MOVEMENT_MODE_ABS); // startflag off
            }
            else
            {
                SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag,
                    ControllerOutputData.MOVEMENT_MODE_RELATIVE); // startflag off
            }
            // (2)
            // Input the step data No. to be specified by Word0, bit0-5:IN0-5 E.g.)
            // Specify step data No.1→bit0:IN0 = ON bit1-5:IN1-5 = OFF This is the Base step No that will be used.
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.SVON | ControllerOutputData.IN0); // step data No.1 
            
            // (3)
            // Specify the numerical operation input flags which control the numerical operation data to be entered, by Word1, bit4-15.
            // Turn ON the relevant flag which must be numerically input into the specified step data and turn OFF the relevant flag which is not required.
            // E.g.) Only [position] of the numerical operation data input flag must be specified. → Word1, bit6=ON, Word1, bit4-5,7-15=OFF.
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W1ControllingOfTheControllerAndNumericalDataFlag, ControllerOutputData.ALL_NUMERICAL_DATA); // all
            
            // (4)
            // Input Word2, bit8-9:Movement mode and Word3-17:Numerical operation data.
            // E.g.) Input [Position] 50.00 [mm]. 5000[0.01mm]=(00001388)h→
            // Word4: Target position(L) = (1388)h
            // Word5: Target position (H) = (0000)h
            
            if(MovementMode == MovementMode.Absolute)
            {
                SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag,
                    ControllerOutputData.MOVEMENT_MODE_ABS); // absolute, startflag off
            }
            else
            {
                SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag,
                    ControllerOutputData.MOVEMENT_MODE_RELATIVE); // relative, startflag off
            }
            
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W3Speed, MovementParameters.Speed);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W4TargetPosition, MovementParameters.TargetPosition);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W6Acceleration, MovementParameters.Acceleration);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W7Deceleration, MovementParameters.Deceleration);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W11MovingForce, MovementParameters.PushingForce);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W9TriggerLv, MovementParameters.TriggerLv);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W10PushingSpeed, MovementParameters.PushingSpeed);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W11MovingForce, MovementParameters.PushingForceForPositioning);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W12Area1, MovementParameters.Area1);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W14Area2, MovementParameters.Area2);
            SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W16InPosition, MovementParameters.PositioningWidth);  
            
            // (5)
            // Input the numerical operation data input flag bit and numerical operation data, and then input Word2,
            // bit0: Start flag = ON. The numerical operation data will be transmitted when the start flag is ON.
            //SetOutputValue(OutputAreaMapping.W2MovementModeAndStartFlag, START_FLAG | MOVEMENT_MODE_ABS); // absolute, set start flag
            if(MovementMode == MovementMode.Absolute)
            {
                SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag,
                    ControllerOutputData.START_FLAG | ControllerOutputData.MOVEMENT_MODE_ABS); // absolute, startflag off
            }
            else
            {
                SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag,
                    ControllerOutputData.START_FLAG | ControllerOutputData.MOVEMENT_MODE_RELATIVE); // relative, startflag off
            }
            
            //(6)
            // When the actuator starts operating, Word0, bit8: BUSY = ON will be output. Then, input Word2, bit0: Start flag = OFF.
            WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, ControllerInputData.BUSY, true);
            if(MovementMode == MovementMode.Absolute)
            {
                SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag,
                    ControllerOutputData.MOVEMENT_MODE_ABS); // startflag off
            }
            else
            {
                SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W2MovementModeAndStartFlag,
                    ControllerOutputData.MOVEMENT_MODE_RELATIVE); // startflag off
            }
            
            // (7) When the actuator reached the target position, Word0, bit11: INP=ON is output.
            // (Refer to "INP" section (P.34) for signal ON conditions) When the actuator stops, Word0, bit8: BUSY=OFF will be output.
            // The completion of the actuator operation is validated when both Word0, bit11: INP=ON and Word0, bit8: BUSY=OFF are established.
            WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, ControllerInputData.INP, true); // INP
            WaitForFlag(InputAreaMapping.W0InputPortToWhichSignalsAreAllocated, ControllerInputData.BUSY, false); // BUSY
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Error: {ex.Message}");
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
                int inputPort = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W0InputPortToWhichSignalsAreAllocated);
                var val = inputPort & bitMask;
                bool isConditionMet = (val > 0);
                if (isConditionMet == awaitedValue)
                    return;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error: {ex.Message}");
            }
            Thread.Sleep(10);
        }

    }
    public void Disconnect()
    {
        _eeipClient.UnRegisterSession();
    }
}