using Sres.Net.EEIP;

namespace SMCAxisController.DataModel;

public static class SmcOutputHelper
{
    private const int _outputInstance = 150; // p.21
    
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
    
    public static void SetOutputValue(EEIPClient eeipClient, OutputAreaMapping outputAreaMapping, int bitMask)
    {
        // Retrieve the entire output data buffer
        var outputData = eeipClient.AssemblyObject.getInstance(_outputInstance);

        // Modify the selected output value
        var stepInfo = OutputMemoryDictionary[outputAreaMapping];

        // Validate data length
        if (outputData == null || outputData.Length < stepInfo.Offset + stepInfo.Size)
            throw new ArgumentException("Output data array is too small for the requested OutputAreaMapping.");

        // Write the value based on size
        switch (stepInfo.Size)
        {
            case 2: // 16-bit value
                Array.Copy(BitConverter.GetBytes((short)bitMask), 0, outputData, stepInfo.Offset, 2);
                break;

            case 4: // 32-bit value
                Array.Copy(BitConverter.GetBytes(bitMask), 0, outputData, stepInfo.Offset, 4);
                break;

            default:
                throw new NotSupportedException($"Unsupported size {stepInfo.Size} for OutputAreaMapping {outputAreaMapping}");
        }

        // Send the modified output data back to the device
        eeipClient.AssemblyObject.setInstance(_outputInstance, outputData);

        //_logger.LogDebug($"Set {outputAreaMapping} to {value} and updated all outputs.");
    }
    
    public static void SetOutputValue(byte[] data, OutputAreaMapping outputAreaMapping, int bitMask)
    {
        var stepInfo = OutputMemoryDictionary[outputAreaMapping];

        // Validate data length
        if (data == null || data.Length < stepInfo.Offset + stepInfo.Size)
            throw new ArgumentException("Data array is too small or null for the requested OutputAreaMapping.");

        // Write data based on size
        switch (stepInfo.Size)
        {
            case 2: // 16-bit value
                Array.Copy(BitConverter.GetBytes((short)bitMask), 0, data, stepInfo.Offset, 2);
                break;

            case 4: // 32-bit value
                Array.Copy(BitConverter.GetBytes(bitMask), 0, data, stepInfo.Offset, 4);
                break;

            default:
                throw new NotSupportedException($"Unsupported size {stepInfo.Size} for OutputAreaMapping {outputAreaMapping}");
        }
    }
    
    public static int ParseOutputValue(byte[] data, OutputAreaMapping outputAreaMapping)
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
    
    public static ControllerOutputData GetAllOutputs(EEIPClient eeipClient)
    {
        var outputData = eeipClient.AssemblyObject.getInstance(_outputInstance);
        var controllerOutputData = new ControllerOutputData();

        controllerOutputData.OutputPortToWhichSignalsAreAllocated = ParseOutputValue(outputData, OutputAreaMapping.W0OutputPortToWhichSignalsAreAllocated);
        controllerOutputData.ControllingOfTheControllerAndNumericalDataFlag = ParseOutputValue(outputData, OutputAreaMapping.W1ControllingOfTheControllerAndNumericalDataFlag);
        controllerOutputData.MovementModeAndStartFlag = ParseOutputValue(outputData, OutputAreaMapping.W2MovementModeAndStartFlag);
        controllerOutputData.Speed = ParseOutputValue(outputData, OutputAreaMapping.W3Speed);
        controllerOutputData.TargetPosition = ParseOutputValue(outputData, OutputAreaMapping.W4TargetPosition);
        controllerOutputData.Acceleration = ParseOutputValue(outputData, OutputAreaMapping.W6Acceleration);
        controllerOutputData.Deceleration = ParseOutputValue(outputData, OutputAreaMapping.W7Deceleration);
        controllerOutputData.PushingForceThrustSettingValue = ParseOutputValue(outputData, OutputAreaMapping.W8PushingForceThrustSettingValue);
        controllerOutputData.TriggerLv = ParseOutputValue(outputData, OutputAreaMapping.W9TriggerLv);
        controllerOutputData.PushingSpeed = ParseOutputValue(outputData, OutputAreaMapping.W10PushingSpeed);
        controllerOutputData.PushingForce = ParseOutputValue(outputData, OutputAreaMapping.W11MovingForce);
        controllerOutputData.Area1 = ParseOutputValue(outputData, OutputAreaMapping.W12Area1);
        controllerOutputData.Area2 = ParseOutputValue(outputData, OutputAreaMapping.W14Area2);
        controllerOutputData.InPosition = ParseOutputValue(outputData, OutputAreaMapping.W16InPosition);

        return controllerOutputData;
    }
}