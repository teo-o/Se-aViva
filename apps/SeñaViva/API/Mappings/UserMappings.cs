using SenaViva.Shared.DTOs.Responses;
using SenaViva.Shared.Entities;

namespace SenaViva.API.Mappings;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.Auth0Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt);
}
