using System.ComponentModel.DataAnnotations;
using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Requests;

public class UpdateConversationSessionRequest
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public SessionStatus? Status { get; set; }

    [Required(ErrorMessage = "La lengua de señas es obligatoria.")]
    public SignLanguage? SignLanguage { get; set; }
}
