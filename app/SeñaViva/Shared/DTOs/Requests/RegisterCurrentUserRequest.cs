using System.ComponentModel.DataAnnotations;
using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Requests;

public class RegisterCurrentUserRequest
{
    [Required(ErrorMessage = "Selecciona cómo te comunicas.")]
    [AllowedValues(UserRole.Deaf, UserRole.Hearing, UserRole.Administrator, null, ErrorMessage = "En el registro solo puedes elegir persona sorda u oyente.")]
    public UserRole? Role { get; set; }
}
