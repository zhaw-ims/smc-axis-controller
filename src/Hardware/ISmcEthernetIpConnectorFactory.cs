namespace SMCAxisController.Hardware;

public interface ISmcEthernetIpConnectorFactory
{
    ISmcEthernetIpConnector GetNotConnectedSmcEthernetIpConnector();
    
    List<ISmcEthernetIpConnector> GetAllSmcEthernetIpConnectors();
    ISmcEthernetIpConnector GetSmcEthernetIpConnectorByName(string name);
}