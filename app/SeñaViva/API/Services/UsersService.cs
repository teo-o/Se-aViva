using Microsoft.EntityFrameworkCore;
using SenaViva.API.Auth;
using SenaViva.API.Data;
using SenaViva.API.Errors;
using SenaViva.API.Mappings;
using SenaViva.Shared.Auth;
using SenaViva.Shared.DTOs.Requests;
using SenaViva.Shared.DTOs.Responses;
using SenaViva.Shared.Entities;

namespace SenaViva.API.Services;

public class UsersService(DataContext context)
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await context.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);

        return users.Select(user => user.ToResponse()).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            ?? throw UserNotFound(id);

        return user.ToResponse();
    }

    public async Task<UserResponse> GetByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Auth0Id == auth0Id, cancellationToken)
            ?? throw new NotFoundException("El usuario autenticado aún no está registrado en SeñaViva.");

        return user.ToResponse();
    }

    public async Task<(UserResponse User, bool Created)> RegisterCurrentUserAsync(
        CurrentUser currentUser,
        RegisterCurrentUserRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Auth0Id == currentUser.Auth0Id, cancellationToken);

        if (existing is not null)
        {
            return (existing.ToResponse(), false);
        }

        if (string.IsNullOrWhiteSpace(currentUser.Email))
        {
            throw new BadRequestException(
                $"El token no incluye el claim '{SenaVivaClaims.Email}'. Revisa la Action post-login de SeñaViva en Auth0.");
        }

        var email = NormalizeEmail(currentUser.Email);

        // Accounts are never linked by email: an unverified Auth0 email could otherwise take over an existing record.
        if (await context.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new ConflictException(
                $"Ya existe un usuario con el email '{email}'. Un administrador debe enlazarlo asignándole su Auth0 ID.");
        }

        var user = new User
        {
            Auth0Id = currentUser.Auth0Id,
            Email = email,
            Name = string.IsNullOrWhiteSpace(currentUser.FullName) ? email : currentUser.FullName.Trim(),
            Role = request.Role!.Value,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return (user.ToResponse(), true);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var auth0Id = NormalizeAuth0Id(request.Auth0Id);
        await EnsureUniqueAsync(email, auth0Id, excludedUserId: null, cancellationToken);

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            Role = request.Role!.Value,
            Auth0Id = auth0Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return user.ToResponse();
    }

    public async Task UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([id], cancellationToken) ?? throw UserNotFound(id);

        var email = NormalizeEmail(request.Email);
        var auth0Id = NormalizeAuth0Id(request.Auth0Id);
        await EnsureUniqueAsync(email, auth0Id, excludedUserId: id, cancellationToken);

        user.Name = request.Name.Trim();
        user.Email = email;
        user.Role = request.Role!.Value;
        user.IsActive = request.IsActive!.Value;
        user.Auth0Id = auth0Id;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([id], cancellationToken) ?? throw UserNotFound(id);

        var hasSessions = await context.ConversationSessions.AnyAsync(session => session.UserId == id, cancellationToken);
        if (hasSessions)
        {
            throw new ConflictException($"El usuario {id} tiene sesiones de conversación y no se puede eliminar. Desactívalo en su lugar.");
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueAsync(string email, string? auth0Id, int? excludedUserId, CancellationToken cancellationToken)
    {
        var otherUsers = context.Users.Where(user => user.Id != excludedUserId);

        if (await otherUsers.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new ConflictException($"Ya existe un usuario con el email '{email}'.");
        }

        if (auth0Id is not null && await otherUsers.AnyAsync(user => user.Auth0Id == auth0Id, cancellationToken))
        {
            throw new ConflictException($"Ya existe un usuario enlazado al Auth0 ID '{auth0Id}'.");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string? NormalizeAuth0Id(string? auth0Id) =>
        string.IsNullOrWhiteSpace(auth0Id) ? null : auth0Id.Trim();

    private static NotFoundException UserNotFound(int id) => new($"No existe el usuario {id}.");
}
