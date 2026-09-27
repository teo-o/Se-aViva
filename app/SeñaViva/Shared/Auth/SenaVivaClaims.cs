namespace SenaViva.Shared.Auth;

// Custom claims added to Auth0 tokens by the post-login Action (see auth0/actions/perfil-senaviva.js).
public static class SenaVivaClaims
{
    private const string Namespace = "https://senaviva.local/";

    public const string Email = Namespace + "email";
    public const string FullName = Namespace + "nombre_completo";
    public const string DocumentType = Namespace + "tipo_documento";
    public const string DocumentNumber = Namespace + "numero_documento";
    public const string UserType = Namespace + "tipo_usuario";
    public const string Roles = Namespace + "roles";

    public const string AdministratorRole = "Administrador";
}
