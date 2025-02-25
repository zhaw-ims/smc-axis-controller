using Microsoft.AspNetCore.Mvc;
using SMCAxisController.DataModel;
using SMCAxisController.Hardware;
using SMCAxisController.StateMachine;

namespace SMCAxisController.API;

[ApiController]
[Route("api/[controller]/[action]")]
public class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IStateMachine _stateMachine;
    
    public ApiController(
        ILogger<ApiController> logger,
        IConnectorsRepository connectorsRepository,
        IStateMachine stateMachine
        )

    {
        _logger = logger;
        _connectorsRepository = connectorsRepository;
        _stateMachine = stateMachine;
    }

    [HttpGet]
    public async Task<ActionResult<ControllerInputData>> GetControllerData([FromQuery] string name)
    {
        var ret = _connectorsRepository.GetSmcEthernetIpConnectorByName(name);

        if (ret == null)
        {
            return NotFound(new { message = $"No controller found with name: {name}" });
        }

        return Ok(ret.ControllerInputData);
    }
    
    [HttpPost]
    public async Task<ActionResult> MoveToPosition([FromQuery] string name, [FromBody] ControllerOutputData ControllerOutputData)
    {
        var actuator = _connectorsRepository.GetSmcEthernetIpConnectorByName(name);

        if (actuator == null)
        {
            return NotFound(new { message = $"No actuator found with name: {name}" });
        }

        actuator.ControllerOutputData = ControllerOutputData;
        actuator.GoToPositionNumerical();

        // if (!success)
        // {
        //     return BadRequest(new { message = "Failed to move actuator to position." });
        // }

        return Ok(new { message = $"Command to move actuator '{name}' was invoked successfully.", targetPosition = ControllerOutputData.TargetPosition });
    }
    
    [HttpPost]
    public async Task<ActionResult> RunFlow([FromQuery] string name)
    {
        _stateMachine.FireRunFlow(name);
        
        return Ok(new { message = $"Flow '{name}' run successfully."});
    }
    
    [HttpPost]
    public async Task<ActionResult> RunSequence([FromQuery] string name)
    {
        _stateMachine.FireRunSequence(name);
        
        return Ok(new { message = $"Sequence '{name}' run successfully."});
    }
    
    [HttpGet]
    public async Task<ActionResult> GetState()
    {
        var state = _stateMachine.State.ToString();
    
        return Ok(new { message = state });
    }

}