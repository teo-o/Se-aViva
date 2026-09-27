using SenaViva.Shared.Enums;

namespace SenaViva.Shared.Entities;

public class User
{
    public int Id { get; set; }

    public string? Auth0Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<ConversationSession> ConversationSessions { get; set; } = [];
}
