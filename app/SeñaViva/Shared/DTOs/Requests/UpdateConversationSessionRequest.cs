using System.ComponentModel.DataAnnotations;
using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Requests;

public class UpdateConversationSessionRequest
{
    [Required]
    public SessionStatus? Status { get; init; }

    [Required]
    public SignLanguage? SignLanguage { get; init; }
}
