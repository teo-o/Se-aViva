using SenaViva.Shared.Enums;

namespace SenaViva.Web.Display;

public static class Labels
{
    public static string ToDisplayName(this UserRole role) => role switch
    {
        UserRole.Deaf => "Persona sorda",
        UserRole.Hearing => "Persona oyente",
        UserRole.Interpreter => "Intérprete",
        UserRole.Administrator => "Administrador",
        _ => role.ToString()
    };

    public static string ToDisplayName(this SessionStatus status) => status switch
    {
        SessionStatus.Active => "Activa",
        SessionStatus.Finished => "Finalizada",
        _ => status.ToString()
    };

    public static string ToDisplayName(this SignLanguage language) => language switch
    {
        SignLanguage.LSC => "Lengua de Señas Colombiana (LSC)",
        _ => language.ToString()
    };

    public static string DocumentTypeName(string? code) => code switch
    {
        "CC" => "Cédula de ciudadanía",
        "TI" => "Tarjeta de identidad",
        "CE" => "Cédula de extranjería",
        "PA" => "Pasaporte",
        "PPT" => "Permiso por Protección Temporal",
        null or "" => "Sin registrar",
        _ => code
    };

    public static string ToLocalDisplay(this DateTimeOffset value) => value.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
}
