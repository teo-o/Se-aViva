using SenaViva.Shared.DTOs.Responses;
using SenaViva.Shared.Entities;

namespace SenaViva.API.Mappings;

public static class ConversationSessionMappings
{
    public static ConversationSessionResponse ToResponse(this ConversationSession session) =>
        new(session.Id, session.StartedAt, session.Status, session.SignLanguage, session.UserId, session.User!.Name);
}
