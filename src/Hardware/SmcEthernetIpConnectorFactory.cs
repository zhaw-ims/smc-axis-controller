using SMCAxisController.DataModel;

namespace SMCAxisController.Hardware;

public class SmcEthernetIpConnectorFactory : ISmcEthernetIpConnectorFactory
{
    private readonly IEnumerable<ISmcEthernetIpConnector> _smcEthernetIpConnectors;
    
    public SmcEthernetIpConnectorFactory(IEnumerable<ISmcEthernetIpConnector> smcEthernetIpConnectors)
    {
        _smcEthernetIpConnectors = smcEthernetIpConnectors;
    }
    
    public ISmcEthernetIpConnector GetNotConnectedSmcEthernetIpConnector()
    {
        var smcEthernetIpConnector = _smcEthernetIpConnectors.FirstOrDefault(c => c.Status == ControllerStatus.NotConnected);

        if (smcEthernetIpConnector == null)
            throw new InvalidOperationException("No unassigned connector found");

        return smcEthernetIpConnector;
    }

    public ISmcEthernetIpConnector GetSmcEthernetIpConnectorByName(string name)
    {
        var smcEthernetIpConnector = _smcEthernetIpConnectors.FirstOrDefault(c => c.ControllerProperties.Name == name);

        if (smcEthernetIpConnector == null)
            throw new InvalidOperationException("No unassigned connector found");

        return smcEthernetIpConnector;    
    }

    public List<ISmcEthernetIpConnector> GetAllSmcEthernetIpConnectors()
    {
        return _smcEthernetIpConnectors.ToList();
    }
}