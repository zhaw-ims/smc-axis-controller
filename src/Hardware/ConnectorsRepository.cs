namespace SMCAxisController.Hardware;

public class ConnectorsRepository : IConnectorsRepository
{
    public List<ISmcEthernetIpConnector> SmcEthernetIpConnectors{get; set; } = new List<ISmcEthernetIpConnector>();

    public ISmcEthernetIpConnector GetSmcEthernetIpConnectorByName(string name)
    {
        return SmcEthernetIpConnectors.FirstOrDefault(c => c.ControllerProperties.Name == name);
    }
}