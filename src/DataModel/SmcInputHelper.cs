using Sres.Net.EEIP;

namespace SMCAxisController.DataModel;

public static class SmcInputHelper
{
    private const int _inputInstance = 100;
    
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
    public static int GetInputValue(byte[] data, InputAreaMapping inputAreaMapping)
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
    
    public static ControllerInputData GetAllInputs(EEIPClient eeipClient)
    {
        var inputData = eeipClient.AssemblyObject.getInstance(_inputInstance);
        var controllerInputData = new ControllerInputData();
        
        controllerInputData.InputPort = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W0InputPortToWhichSignalsAreAllocated);
        controllerInputData.ControllerInformationFlag = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W1ControllerInformationFlag);
        controllerInputData.CurrentPosition = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W2CurrentPosition);
        controllerInputData.CurrentSpeed = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W4CurrentSpeed);
        controllerInputData.CurrentPushingForce = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W5CurrentPushingForce);
        controllerInputData.TargetPosition = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W6TargetPosition);
        controllerInputData.Alarm1And2 = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W7Alarm1And2);
        controllerInputData.Alarm3And4 = SmcInputHelper.GetInputValue(inputData, InputAreaMapping.W9Alarm3And4);

        return controllerInputData;
    }
}