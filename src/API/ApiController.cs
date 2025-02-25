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
    
    [HttpPost]
    public async Task<ActionResult> PowerOnAllControllers()
    {
        if(_stateMachine.CanFire(RobotTrigger.PowerOnAll))
        {
            _stateMachine.Fire(RobotTrigger.PowerOnAll);
            return Ok(new { message = "PowerOnAllControllers run successfully." });
        }
        else
        {
            return Conflict(new { message = "Cannot execute PowerOnAllControllers." });
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> ReturnAllActuatorsToOrigin()
    {
        if(_stateMachine.CanFire(RobotTrigger.ReturnAllToOrigin))
        {
            _stateMachine.Fire(RobotTrigger.ReturnAllToOrigin);
            return Ok(new { message = "ReturnAllActuatorsToOrigin run successfully." });
        }
        else
        {
            return Conflict(new { message = "Cannot execute ReturnAllActuatorsToOrigin." });
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> RunSequence([FromQuery] string name)
    {
        if (_stateMachine.CanFire(RobotTrigger.RunSequence))
        {
            _stateMachine.FireRunSequence(name);
            return Ok(new { message = $"Sequence '{name}' run successfully."});
        }
        else
        {
            return Conflict(new { message = "Cannot execute RunSequence." });    
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> RunFlow([FromQuery] string name)
    {
        if (_stateMachine.CanFire(RobotTrigger.RunFlow))
        {
            _stateMachine.FireRunFlow(name);
            return Ok(new { message = $"Flow '{name}' run successfully."});
        }
        else
        {
            return Conflict(new { message = "Cannot execute RunFlow." });    
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> ClearError()
    {
        if (_stateMachine.CanFire(RobotTrigger.ClearError))
        {
            _stateMachine.Fire(RobotTrigger.ClearError);
            return Ok(new { message = $"ClearError run successfully."});
        }
        else
        {
            return Conflict(new { message = "Cannot execute ClearError." });    
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> ResetAlarm([FromQuery] string controllerName)
    {
        var controller = _connectorsRepository.GetSmcEthernetIpConnectorByName(controllerName);

        if (controller == null)
        {
            return NotFound(new { message = $"No controller found with name: {controllerName}" });
        }

        controller.AlarmReset();
        
        return Ok(new { message = $"Command to reset alarm of {controllerName} controller was invoked successfully."});
    }
    
    [HttpPost]
    public async Task<ActionResult> MoveToPosition([FromQuery] string name, [FromBody] ControllerOutputData ControllerOutputData)
    {
        var controller = _connectorsRepository.GetSmcEthernetIpConnectorByName(name);

        if (controller == null)
        {
            return NotFound(new { message = $"No actuator found with name: {name}" });
        }

        controller.ControllerOutputData = ControllerOutputData;
        controller.GoToPositionNumerical();

        // if (!success)
        // {
        //     return BadRequest(new { message = "Failed to move actuator to position." });
        // }

        return Ok(new { message = $"Command to move actuator '{name}' was invoked successfully.", targetPosition = ControllerOutputData.TargetPosition });
    }
    
    [HttpGet]
    public async Task<ActionResult> GetState()
    {
        var state = _stateMachine.State.ToString();
    
        return Ok(new { state });
    }
    
    [HttpGet]
    public async Task<ActionResult> GetError()
    {
        return Ok(new { _stateMachine.LastError });
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

}