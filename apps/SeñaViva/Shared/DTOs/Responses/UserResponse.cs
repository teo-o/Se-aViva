using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Responses;

public record UserResponse(
    int Id,
    string? Auth0Id,
    string Name,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTimeOffset CreatedAt);
