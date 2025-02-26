using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    private readonly RobotSequences _robotSequences;
    
    public ApiController(
        ILogger<ApiController> logger,
        IConnectorsRepository connectorsRepository,
        IStateMachine stateMachine,
        RobotSequences robotSequences
        )

    {
        _logger = logger;
        _connectorsRepository = connectorsRepository;
        _stateMachine = stateMachine;
        _robotSequences = robotSequences;
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
        
        if(controller.Status == ControllerStatus.Connected)
        {
            controller.AlarmReset();
            return Ok(new { message = $"Command to reset alarm of {controllerName} controller was invoked successfully." });
        }
        else
        {
            return Conflict(new { message = $"Cannot execute ResetAlarm. Controller controllerName is not connected." });
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> MoveToPosition([FromQuery] string controllerName, [FromBody] ControllerOutputData ControllerOutputData)
    {
        var controller = _connectorsRepository.GetSmcEthernetIpConnectorByName(controllerName);

        if (controller == null)
        {
            return NotFound(new { message = $"No actuator found with name: {controllerName}" });
        }

        controller.ControllerOutputData = ControllerOutputData;
        
        if(controller.Status == ControllerStatus.Connected)
        {
            controller.GoToPositionNumerical();
            return Ok(new
            {
                message = $"Command to move actuator '{controllerName}' was invoked successfully.",
                targetPosition = ControllerOutputData.TargetPosition
            });
        }
        else
        {
            return Conflict(new { message = $"Cannot execute MoveToPosition. Controller {controllerName} is not connected." });
        }
        
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
    
    [HttpGet]
    public async Task<ActionResult<ControllerInputData>> GetAllFlows()
    {
        var ret = _robotSequences.SequenceFlows.Values.Select(sf => sf.Name).ToList();
        if (ret == null)
        {
            return NotFound(new { message = $"No flows found" });
        }
        return Ok(ret);
    }

    [HttpGet]
    public async Task<ActionResult<ControllerInputData>> GetAllSequences()
    {
        var ret = _robotSequences.DefinedSequences.Values.Select(ms => ms.Name).ToList();
        if (ret == null)
        {
            return NotFound(new { message = $"No sequences found" });
        }
        
        return Ok(ret);
    }
}