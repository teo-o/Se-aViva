using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SenaViva.API.Auth;
using SenaViva.API.Services;
using SenaViva.Shared.Auth;
using SenaViva.Shared.DTOs.Requests;
using SenaViva.Shared.DTOs.Responses;
using SenaViva.Shared.Enums;

namespace SenaViva.API.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController(UsersService usersService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.ReadUsers)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await usersService.GetAllAsync(cancellationToken));

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetCurrent(CancellationToken cancellationToken)
    {
        var currentUser = CurrentUser.From(User);
        if (currentUser.IsMachine)
        {
            return Forbid();
        }

        return Ok(await usersService.GetByAuth0IdAsync(currentUser.Auth0Id, cancellationToken));
    }

    [HttpPost("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> RegisterCurrent(
        RegisterCurrentUserRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = CurrentUser.From(User);
        if (currentUser.IsMachine)
        {
            return Forbid();
        }

        if (request.Role == UserRole.Administrator && !currentUser.IsAdministrator)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Prohibido",
                detail: $"Solo quien tiene el rol '{SenaVivaClaims.AdministratorRole}' en Auth0 puede registrarse como administrador.");
        }

        var (user, created) = await usersService.RegisterCurrentUserAsync(currentUser, request, cancellationToken);
        return created ? CreatedAtAction(nameof(GetCurrent), user) : Ok(user);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.ReadUsers)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await usersService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permissions.WriteUsers)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await usersService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.WriteUsers)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await usersService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.WriteUsers)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await usersService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
