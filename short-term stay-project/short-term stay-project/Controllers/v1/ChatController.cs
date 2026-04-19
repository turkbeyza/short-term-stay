using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using short_term_stay_project.Services.Agent;

namespace short_term_stay_project.Controllers.v1;

public record ChatRequest(string Message, string SessionId);

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IGeminiAgentService _agentService;

    public ChatController(IGeminiAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {


        if (string.IsNullOrWhiteSpace(request.SessionId))
            request = request with { SessionId = Guid.NewGuid().ToString() };

        var reply = await _agentService.ChatAsync(request.Message, request.SessionId);

        return Ok(new { Reply = reply, SessionId = request.SessionId });
    }
}
