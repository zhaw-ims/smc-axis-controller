namespace SMCAxisController.DataModel;

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