using Microsoft.EntityFrameworkCore;
using SenaViva.API.Data;
using SenaViva.API.Errors;
using SenaViva.API.Mappings;
using SenaViva.Shared.DTOs.Requests;
using SenaViva.Shared.DTOs.Responses;
using SenaViva.Shared.Entities;
using SenaViva.Shared.Enums;

namespace SenaViva.API.Services;

public class ConversationSessionsService(DataContext context)
{
    public async Task<IReadOnlyList<ConversationSessionResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var sessions = await context.ConversationSessions
            .AsNoTracking()
            .Include(session => session.User)
            .OrderByDescending(session => session.StartedAt)
            .ToListAsync(cancellationToken);

        return sessions.Select(session => session.ToResponse()).ToList();
    }

    public async Task<ConversationSessionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await context.ConversationSessions
            .AsNoTracking()
            .Include(session => session.User)
            .FirstOrDefaultAsync(session => session.Id == id, cancellationToken)
            ?? throw SessionNotFound(id);

        return session.ToResponse();
    }

    public async Task<ConversationSessionResponse> CreateAsync(
        CreateConversationSessionRequest request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([request.UserId], cancellationToken)
            ?? throw new NotFoundException($"User {request.UserId} was not found.");

        var session = new ConversationSession
        {
            Id = Guid.NewGuid(),
            StartedAt = DateTimeOffset.UtcNow,
            Status = SessionStatus.Active,
            SignLanguage = request.SignLanguage,
            User = user
        };

        context.ConversationSessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);

        return session.ToResponse();
    }

    public async Task UpdateAsync(Guid id, UpdateConversationSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await context.ConversationSessions.FindAsync([id], cancellationToken) ?? throw SessionNotFound(id);

        session.Status = request.Status!.Value;
        session.SignLanguage = request.SignLanguage!.Value;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await context.ConversationSessions.FindAsync([id], cancellationToken) ?? throw SessionNotFound(id);

        context.ConversationSessions.Remove(session);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static NotFoundException SessionNotFound(Guid id) => new($"Conversation session {id} was not found.");
}
