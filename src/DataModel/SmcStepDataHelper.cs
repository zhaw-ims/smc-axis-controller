using Sres.Net.EEIP;

namespace SMCAxisController.DataModel;

public static class SmcStepDataHelper
{
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
    public static void SetStepDataValue(StepData outputMemory, int value, byte[] stepData)
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
        

        //_logger.LogDebug($"Set {outputMemory} to {value} and updated all outputs.");
    }
    public static int GetStepDataValue(byte[] data, StepData stepData)
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
    
    public static MovementParameters GetStepData(EEIPClient eeipClient, int stepNumber)
    {
        var stepData = eeipClient.GetAttributeSingle(0x67, stepNumber, 100);
        var movementParameters = new MovementParameters();

        movementParameters.MovementMode = (MovementMode)SmcStepDataHelper.GetStepDataValue(stepData, StepData.MovementMode);
        movementParameters.Speed = SmcStepDataHelper.GetStepDataValue(stepData, StepData.Speed);
        movementParameters.TargetPosition = SmcStepDataHelper.GetStepDataValue(stepData, StepData.TargetPosition);
        movementParameters.Acceleration = SmcStepDataHelper.GetStepDataValue(stepData, StepData.Acceleration);
        movementParameters.Deceleration = SmcStepDataHelper.GetStepDataValue(stepData, StepData.Deceleration);
        movementParameters.PushingForce = SmcStepDataHelper.GetStepDataValue(stepData, StepData.PushingForce);
        movementParameters.TriggerLv = SmcStepDataHelper.GetStepDataValue(stepData, StepData.TriggerLv);
        movementParameters.PushingSpeed = SmcStepDataHelper.GetStepDataValue(stepData, StepData.PushingSpeed);
        movementParameters.PushingForceForPositioning = SmcStepDataHelper.GetStepDataValue(stepData, StepData.PushingForceForPositioning);
        movementParameters.Area1 = SmcStepDataHelper.GetStepDataValue(stepData, StepData.Area1);
        movementParameters.Area2 = SmcStepDataHelper.GetStepDataValue(stepData, StepData.Area2);
        movementParameters.PositioningWidth = SmcStepDataHelper.GetStepDataValue(stepData, StepData.PositioningWidth);

        //_logger.LogDebug($"\nStep Data: {stepNumber}\n{movementParameters}");
        return movementParameters;
    }
    
    public static void SetStepData(EEIPClient eeipClient, int stepNumber, MovementParameters movementParameters)
    {
        byte[] stepDataArray = new byte[32];

        SetStepDataValue(StepData.MovementMode, (int)movementParameters.MovementMode, stepDataArray);
        SetStepDataValue(StepData.Speed, movementParameters.Speed, stepDataArray);
        SetStepDataValue(StepData.TargetPosition, movementParameters.TargetPosition, stepDataArray);
        SetStepDataValue(StepData.Acceleration, movementParameters.Acceleration, stepDataArray);
        SetStepDataValue(StepData.Deceleration, movementParameters.Deceleration, stepDataArray);
        SetStepDataValue(StepData.PushingForce, movementParameters.PushingForce, stepDataArray);
        SetStepDataValue(StepData.TriggerLv, movementParameters.TriggerLv, stepDataArray);
        SetStepDataValue(StepData.PushingSpeed, movementParameters.PushingSpeed, stepDataArray);
        SetStepDataValue(StepData.PushingForceForPositioning, movementParameters.PushingForceForPositioning, stepDataArray);
        SetStepDataValue(StepData.Area1, movementParameters.Area1, stepDataArray);
        SetStepDataValue(StepData.Area2, movementParameters.Area2, stepDataArray);
        SetStepDataValue(StepData.PositioningWidth, movementParameters.PositioningWidth, stepDataArray);
        
        eeipClient.SetAttributeSingle(0x67, stepNumber, 100, stepDataArray);
    }
}