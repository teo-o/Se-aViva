using System.ComponentModel.DataAnnotations;
using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Requests;

public class CreateConversationSessionRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El usuario es obligatorio.")]
    public int UserId { get; set; }

    public SignLanguage SignLanguage { get; set; } = SignLanguage.LSC;
}
