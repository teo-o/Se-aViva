namespace SenaViva.API.Auth;

public static class Permissions
{
    public const string ReadUsers = "read:users";
    public const string WriteUsers = "write:users";
    public const string ReadSessions = "read:sessions";
    public const string WriteSessions = "write:sessions";

    public static readonly IReadOnlyList<string> All = [ReadUsers, WriteUsers, ReadSessions, WriteSessions];
}
