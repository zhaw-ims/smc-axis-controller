namespace SMCAxisController.DataModel;

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