using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SenaViva.API.Auth;
using SenaViva.API.Services;
using SenaViva.Shared.DTOs.Requests;
using SenaViva.Shared.DTOs.Responses;

namespace SenaViva.API.Controllers;

[ApiController]
[Route("api/conversation-sessions")]
[Produces("application/json")]
public class ConversationSessionsController(ConversationSessionsService sessionsService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.ReadSessions)]
    public async Task<ActionResult<IReadOnlyList<ConversationSessionResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await sessionsService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.ReadSessions)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationSessionResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await sessionsService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permissions.WriteSessions)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationSessionResponse>> Create(
        CreateConversationSessionRequest request,
        CancellationToken cancellationToken)
    {
        var session = await sessionsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.WriteSessions)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateConversationSessionRequest request,
        CancellationToken cancellationToken)
    {
        await sessionsService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.WriteSessions)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sessionsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
