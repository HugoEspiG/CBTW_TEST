using CBTW_TEST.Domain.Interfaces;
using CBTW_TEST.Domain.Models.Dto;
using Dapr.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace CBTW_TEST.ApiService.Controllers
{
    public class AgentController : ControllerBase
    {
        private readonly ILogger<AgentController> _logger;
        private readonly ILibraryDiscoveryService _libraryDiscoveryService;
        public AgentController(ILogger<AgentController> logger, ILibraryDiscoveryService libraryDiscoveryService)
        {
            _logger = logger;
            _libraryDiscoveryService = libraryDiscoveryService;
        }

        [HttpPost("")]
        public async Task<ActionResult> Create([FromQuery] string messyInput)
        {
            if (string.IsNullOrWhiteSpace(messyInput))
            {
                return BadRequest("Input cannot be empty.");
            }

            try
            {
                _logger.LogInformation("Starting direct AI discovery for input: {Input}", messyInput);

                var result = await _libraryDiscoveryService.ExecuteDiscoveryAsync(messyInput);

                if (!result.WasSuccess)
                {
                    _logger.LogWarning("Discovery failed: {Message}", result.Result);
                    return BadRequest(result);
                }

                _logger.LogInformation("Discovery completed successfully.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Book Match discovery");
                return BadRequest("An internal error occurred while processing your request.");
            }
        }

    }
}
