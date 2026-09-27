using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Responses;

public record ConversationSessionResponse(
    Guid Id,
    DateTimeOffset StartedAt,
    SessionStatus Status,
    SignLanguage SignLanguage,
    int UserId,
    string UserName);
