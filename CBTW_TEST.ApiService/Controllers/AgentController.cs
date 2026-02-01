using CBTW_TEST.Core.Workflows.Match;
using CBTW_TEST.Domain.Models.Dto;
using Dapr.Workflow;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using static Google.Rpc.Context.AttributeContext.Types;

namespace CBTW_TEST.ApiService.Controllers
{
    public class AgentController : ControllerBase
    {
        private readonly ILogger<AgentController> _logger;
        private readonly DaprWorkflowClient _daprWorkflowClient;

        public AgentController(ILogger<AgentController> logger, DaprWorkflowClient daprWorkflowClient)
        {
            _logger = logger;
            _daprWorkflowClient = daprWorkflowClient;
        }

        [HttpPost("")]
        public async Task<ActionResult> Create([FromQuery] string messyInput)
        {
            try
            {
                var workflowId = Guid.NewGuid().ToString();

                _logger.LogInformation("Scheduling workflow with ID: {WorkflowId}", workflowId);
                await _daprWorkflowClient.ScheduleNewWorkflowAsync(name: nameof(LibraryDiscoveryWorkflow), instanceId: workflowId, input: messyInput);
                WorkflowState state = await _daprWorkflowClient.WaitForWorkflowStartAsync(instanceId: workflowId);

                _logger.LogInformation("Workflow started with ID: {WorkflowId}, Workflow State: {WorkflowState}", workflowId, state);
                return new AcceptedResult($"api/v1/workflows/Status/{workflowId}", new { TrackId = workflowId, WorkflowState = state, StatusPath = $"api/v1/workflows/status/{workflowId}", ResultPath = $"api/v1/workflows/WorkflowResult/{workflowId}" });
            }
            catch (BadHttpRequestException ex)
            {
                _logger.LogError(ex, "Bad request while processing Book Match");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Status/{id}")]
        public async Task<ActionResult<WorkflowState>> GetWorkFlowStatus(string id)
        {
            try
            {
                var wfStatus = await _daprWorkflowClient.GetWorkflowStateAsync(id);
                _logger.LogInformation("Workflow status for {WorkflowId}: Exists={Exists}, IsCompleted={IsCompleted}, FailureDetails={FailureDetails}", id, wfStatus.Exists, wfStatus.IsWorkflowCompleted, wfStatus.FailureDetails);
                return Ok(wfStatus);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"No Workflow exists with id: {id}");
                return BadRequest($"No Workflow exists with id: {id}");
            }
        }

        [HttpGet("WorkflowResult/{id}")]
        public async Task<ActionResult<WorkflowResultDto>> GetActionResultAsync(string id)
        {
            try
            {
                var wfStatus = await _daprWorkflowClient.GetWorkflowStateAsync(id);
                if (wfStatus.Exists && wfStatus.FailureDetails != null)
                {
                    _logger.LogError("Workflow {WorkflowId} failed with error: {ErrorMessage}", id, wfStatus.FailureDetails.ErrorMessage);
                    return BadRequest(wfStatus.FailureDetails.ErrorMessage);
                }

                var wfResult = wfStatus.ReadOutputAs<WorkflowResultDto>();
                if (!wfStatus.Exists)
                {
                    _logger.LogWarning("Workflow {WorkflowId} does not exist.", id);
                    return BadRequest();
                }
                _logger.LogInformation("Workflow {WorkflowId} is still running.", id);
                return Ok(wfResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"No Workflow exists with id: {id}");
                return BadRequest($"No Workflow exists with id: {id}");
            }

        }

    }
}
