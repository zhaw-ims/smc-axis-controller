using Microsoft.Extensions.Options;
using SMCAxisController.DataModel;
using SMCAxisController.Hardware;

namespace SMCAxisController.Pages;

public class IndexVm : IIndexVm
{
    private readonly ISmcEthernetIpConnectorFactory _smcEthernetIpConnectorFactory;
    public List<ControllerProperties> ControllerConfigs { get; }
    public ISmcEthernetIpConnector CurrentSmcEthernetIpConnector { get; private set; }

    public IndexVm(
        ISmcEthernetIpConnectorFactory smcEthernetIpConnectorFactory,
        IOptions<List<ControllerProperties>> controllerConfigs)
    {
        _smcEthernetIpConnectorFactory = smcEthernetIpConnectorFactory;
        ControllerConfigs = controllerConfigs.Value;
    }

    public List<string> GetAllControllerNames()
    {
        var controllerNames = new List<string>();
        foreach (var controllerConfig in ControllerConfigs)
        {
            controllerNames.Add(controllerConfig.Name);
        }
        return controllerNames;
    }
    public void SetControllerByName(string name)
    {
        CurrentSmcEthernetIpConnector = _smcEthernetIpConnectorFactory.GetSmcEthernetIpConnectorByName(name);
    }
}