namespace SMCAxisController.Hardware;

public interface IConnectorsRepository
{
    List<ISmcEthernetIpConnector> SmcEthernetIpConnectors{get; set; }

    ISmcEthernetIpConnector GetSmcEthernetIpConnectorByName(string name);
}