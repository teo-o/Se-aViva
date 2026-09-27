using System.ComponentModel.DataAnnotations;
using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Requests;

public class CreateUserRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [MaxLength(100, ErrorMessage = "El email no puede superar 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public UserRole? Role { get; set; }

    [MaxLength(128, ErrorMessage = "El Auth0 ID no puede superar 128 caracteres.")]
    public string? Auth0Id { get; set; }
}
