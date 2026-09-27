using System.ComponentModel.DataAnnotations;
using SenaViva.Shared.Enums;

namespace SenaViva.Shared.DTOs.Requests;

public class CreateConversationSessionRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; init; }

    public SignLanguage SignLanguage { get; init; } = SignLanguage.LSC;
}
