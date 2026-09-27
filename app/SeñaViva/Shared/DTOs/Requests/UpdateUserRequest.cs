using System.ComponentModel.DataAnnotations;
using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Requests;

public class UpdateUserRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public UserRole? Role { get; init; }

    [Required]
    public bool? IsActive { get; init; }

    [MaxLength(128)]
    public string? Auth0Id { get; init; }
}
