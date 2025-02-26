namespace SMCAxisController.DataModel;

public record ControllerStatusReview
{
    public string ControllerName { get; set; }
    public string Ip { get; set; }
    public bool IsConnected { get; set; }
    public bool IsOriginSetup { get; set; }
    public bool IsAlarm { get; set; }
    public bool IsEstop { get; set; }
    public bool IsPowerOn { get; set; }
}