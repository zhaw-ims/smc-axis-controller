using SMCAxisController.Hardware;

namespace SMCAxisController.Pages;

public interface IIndexVm
{
    ISmcEthernetIpConnector CurrentSmcEthernetIpConnector { get; }
    List<string> GetAllControllerNames();
    void SetControllerByName(string name);
}