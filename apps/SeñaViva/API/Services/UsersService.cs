using Microsoft.EntityFrameworkCore;
using SenaViva.API.Data;
using SenaViva.API.Errors;
using SenaViva.API.Mappings;
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
            throw new ConflictException($"User {id} has conversation sessions and cannot be deleted. Deactivate the user instead.");
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueAsync(string email, string? auth0Id, int? excludedUserId, CancellationToken cancellationToken)
    {
        var otherUsers = context.Users.Where(user => user.Id != excludedUserId);

        if (await otherUsers.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new ConflictException($"A user with email '{email}' already exists.");
        }

        if (auth0Id is not null && await otherUsers.AnyAsync(user => user.Auth0Id == auth0Id, cancellationToken))
        {
            throw new ConflictException($"A user linked to Auth0 id '{auth0Id}' already exists.");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string? NormalizeAuth0Id(string? auth0Id) =>
        string.IsNullOrWhiteSpace(auth0Id) ? null : auth0Id.Trim();

    private static NotFoundException UserNotFound(int id) => new($"User {id} was not found.");
}
