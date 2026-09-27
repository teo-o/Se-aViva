using SenaViva.Shared.Enums;

namespace SenaViva.Shared.Entities;

public class ConversationSession
{
    public Guid Id { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Active;

    public SignLanguage SignLanguage { get; set; } = SignLanguage.LSC;

    public int UserId { get; set; }

    public User? User { get; set; }
}
