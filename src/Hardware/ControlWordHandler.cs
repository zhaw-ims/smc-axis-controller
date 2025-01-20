using System;

namespace SMCAxisController.Hardware;

// Enum representing bit positions in the 16-bit Control Word
[Flags]
public enum ControlWordBits
{
    IN0 = 0,
    IN1 = 1,
    IN2 = 2,
    IN3 = 3,
    IN4 = 4,
    IN5 = 5,
    HOLD = 8,
    SVON = 9,
    DRIVE = 10,
    RESET = 11,
    SETUP = 12,
    JOGMINUS = 13,
    JOGPLUS = 14,
    FLGHT = 15
}

public class ControlWordHandler
{
    // Function to set or clear an individual bit
    public static void SetControlWordBit(ref UInt16 controlWord, ControlWordBits bit, bool value)
    {
        if (value)
            controlWord |= (UInt16)(1 << (int)bit); // Set the bit
        else
            controlWord &= (UInt16)~(1 << (int)bit); // Clear the bit
    }

    // Function to get the value of an individual bit
    public static bool GetControlWordBit(UInt16 controlWord, ControlWordBits bit)
    {
        return (controlWord & (1 << (int)bit)) != 0;
    }

    // Function to set a 6-bit step number (0-63) to bits 0-5
    public static void SetStepNumber(ref UInt16 controlWord, byte stepNumber)
    {
        if (stepNumber > 63) // Validate range
            throw new ArgumentOutOfRangeException(nameof(stepNumber), "Step number must be in the range 0–63.");

        // Clear bits 0–5
        controlWord &= (UInt16)(~0x3F & 0xFFFF);

        // Set the new step number
        controlWord |= (UInt16)(stepNumber & 0x3F);
    }
}
