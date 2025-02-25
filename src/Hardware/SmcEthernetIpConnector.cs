using Sres.Net.EEIP;
using SMCAxisController.DataModel;

namespace SMCAxisController.Hardware;

public class SmcEthernetIpConnector : ISmcEthernetIpConnector 
{
    private readonly ILogger<SmcEthernetIpConnector> _logger;
    private EEIPClient _eeipClient = new EEIPClient();
    private const int _waitingDelay = 50;
    private const int _alarmClearTimeout = 3000;
    private CancellationTokenSource _cancellationTokenSource = new();
    
    public ControllerProperties ControllerProperties { get; set; } = new ControllerProperties();
    public MovementParameters MovementParameters { get; set; } = new MovementParameters();
    public ControllerOutputData ControllerOutputData { get; set; } = new ControllerOutputData();
    public ControllerInputData ControllerInputData { get; set; } = new ControllerInputData();
    public ControllerStatus Status { get; private set; } = ControllerStatus.NotConnected;
    
    public event Func<Task> OnNewControllerData;
    private void NotifyNewControllerData() => OnNewControllerData?.Invoke();
    public event Action<string, MudBlazor.Severity> OnSnackBarMessage;
    private void NotifySnackbar(string message, MudBlazor.Severity severity = MudBlazor.Severity.Normal) => OnSnackBarMessage?.Invoke(message, severity);
    
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
            // _logger.LogDebug(ControllerInputData.ToString());
            ControllerOutputData = SmcOutputHelper.GetAllOutputs(_eeipClient);
            // _logger.LogDebug(ControllerOutputData.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Error: {ex.Message}");
        }
        NotifyNewControllerData();
    }
    public async Task ReturnToOrigin() // [1]
    {
        // (1) Turn the power supply ON.
        
        // (2) Turn ON “SVON”
        // ControllerOutputData.SetSvonAndSend(_eeipClient); // we assume power is turned on separateley
        
        // (3) "SVRE" turns ON.
        // The time when “SVRE” turns ON depends on the type of actuator and the customers application.
        // The actuator with lock is unlocked.
            
        // (4) Turn ON "SETUP".
        ControllerOutputData.SetSetupAndSend(_eeipClient);
        
        // (5) "BUSY" turns ON. (The actuator starts the operation.)
        // After "BUSY" turns ON, "SETUP" will turn OFF.
        // WaitForFlag(() => ControllerInputData.IsBusy()); // not neccessary to check
        
        // (6) "SETON" and "INP" will turn ON. Return to origin is completed when "INP" turns ON.
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        await WaitForFlag(() => ControllerInputData.IsInp(), _cancellationTokenSource.Token);
        
        ControllerOutputData.ClearSetupAndSend(_eeipClient);
    }
    public void PowerOn()
    {
        ControllerOutputData.SetSvonAndSend(_eeipClient);
    }
    public void PowerOff()
    {
        ControllerOutputData.ClearSvonAndSend(_eeipClient);
    }
    public async Task Reset() // [5]
    { 
        // (1)
        // During operation (“BUSY” is ON) “RESET" is turned ON
        ControllerOutputData.SetResetAndSend(_eeipClient);
        
        // (2)
        // “BUSY” and “OUT0” to “OUT5” are OFF.
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        await WaitForFlag(() => ControllerInputData.IsBusy() == false, _cancellationTokenSource.Token);
        
        // (3)
        // The actuator decelerates to stop (controlled).
        
        
        ControllerOutputData.ClearResetAndSend(_eeipClient);
    }
    public void HoldOn()
    {
        // (1)
        // During operation ("BUSY" is ON), turn ON "HOLD".
        ControllerOutputData.SetHoldAndSend(_eeipClient);
        
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
        ControllerOutputData.ClearHoldAndSend(_eeipClient);        
        
        // (4)
        // "BUSY" turns ON. (The actuator restarts.)    
    }
    public async Task AlarmReset()
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
        
        // SmcOutputHelper.SetOutputValue(_eeipClient, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated, ControllerOutputData.OutputPortToWhichSignalsAreAllocated | ControllerOutputData.RESET); 
        ControllerOutputData.SetResetAndSend(_eeipClient);
        // (3)
        // "ALARM" turns OFF, “OUT0” to “OUT3” turn OFF. (The alarm is deactivated.)
        await WaitForAlarmClearAsync();
        
        ControllerOutputData.ClearResetAndSend(_eeipClient);
    }
    async Task WaitForAlarmClearAsync()
    {
        var timeoutTask = Task.Delay(_alarmClearTimeout);
        var alarmTask = Task.Run(async () =>
        {
            while (!ControllerInputData.IsAlarm())
            {
                await Task.Delay(_waitingDelay);
            }
        });

        var completedTask = await Task.WhenAny(alarmTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            NotifySnackbar("Alarm reset timed out", MudBlazor.Severity.Error);
        }
    }
    public async Task GoToPositionNumerical() // Numerical operation p.57
    {
        try
        {
            // (1)
            // Confirm that Word2, bit0: Start flag = OFF. Input Word2, bit0: Start flag = OFF when it is ON.
            ControllerOutputData.ClearStartFlagAndSend(_eeipClient);
            if(MovementParameters.MovementMode == MovementMode.Absolute)
            {
                ControllerOutputData.SetMovementModeAbsAndSend(_eeipClient);
            }
            else
            {
                ControllerOutputData.SetMovementModeRelativeAndSend(_eeipClient);
            }
            
            // (2)
            // Input the step data No. to be specified by Word0, bit0-5:IN0-5 E.g.)
            // Specify step data No.1→bit0:IN0 = ON bit1-5:IN1-5 = OFF This is the Base step No that will be used.
            ControllerOutputData.SetInNumberAndSend(_eeipClient,1);
            
            // (3)
            // Specify the numerical operation input flags which control the numerical operation data to be entered, by Word1, bit4-15.
            // Turn ON the relevant flag which must be numerically input into the specified step data and turn OFF the relevant flag which is not required.
            // E.g.) Only [position] of the numerical operation data input flag must be specified. → Word1, bit6=ON, Word1, bit4-5,7-15=OFF.
            ControllerOutputData.SetNumericalDataFlagsAndSend(_eeipClient, ControllerOutputData.ALL_NUMERICAL_DATA);
            
            // (4)
            // Input Word2, bit8-9:Movement mode and Word3-17:Numerical operation data.
            // E.g.) Input [Position] 50.00 [mm]. 5000[0.01mm]=(00001388)h→
            // Word4: Target position(L) = (1388)h
            // Word5: Target position (H) = (0000)h
            if(MovementParameters.MovementMode == MovementMode.Absolute)
            {
                ControllerOutputData.SetMovementModeAbsAndSend(_eeipClient);
            }
            else
            {
                ControllerOutputData.SetMovementModeRelativeAndSend(_eeipClient);
            }
            ControllerOutputData.SendMovementParameters(_eeipClient, MovementParameters);

            // (5)
            // Input the numerical operation data input flag bit and numerical operation data, and then input Word2,
            // bit0: Start flag = ON. The numerical operation data will be transmitted when the start flag is ON.
            ControllerOutputData.SetStartFlagAndSend(_eeipClient);
            
            //(6)
            // When the actuator starts operating, Word0, bit8: BUSY = ON will be output. Then, input Word2, bit0: Start flag = OFF.
            // await WaitForFlag(() => ControllerInputData.IsBusy()); // this was causing endless waiting
            ControllerOutputData.ClearStartFlagAndSend(_eeipClient);
            
            // (7) When the actuator reached the target position, Word0, bit11: INP=ON is output.
            // (Refer to "INP" section (P.34) for signal ON conditions) When the actuator stops, Word0, bit8: BUSY=OFF will be output.
            // The completion of the actuator operation is validated when both Word0, bit11: INP=ON and Word0, bit8: BUSY=OFF are established.
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            await WaitForFlag(() => ControllerInputData.IsInp(), _cancellationTokenSource.Token);
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            await WaitForFlag(() => ControllerInputData.IsBusy() == false, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Error: {ex.Message}");
        }
    }
    private async Task WaitForFlag(Func<bool> predicate, CancellationToken cancellationToken)
    {
        while (ControllerInputData.IsEstop() == false && ControllerInputData.IsAlarm() == false)
        {
            if (predicate())
            {
                return;
            }
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested, exiting wait loop.");
                return;
            }
            await Task.Delay(_waitingDelay, cancellationToken);
        }
        if (ControllerInputData.IsEstop() && ControllerInputData.IsAlarm() == false)
        {
            _logger.LogInformation("Cancelled due to ESTOP condition.");
        }
        if (ControllerInputData.IsAlarm())
        {
            _logger.LogInformation("Cancelled due to ALARM condition.");
        }
    }
    public void ExitWaitingLoop()
    {
        _cancellationTokenSource.Cancel();    
    }
    public void Disconnect()
    {
        _eeipClient.UnRegisterSession();
    }
}