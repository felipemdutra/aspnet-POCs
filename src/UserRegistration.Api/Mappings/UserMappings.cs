using UserRegistration.Api.Responses;
using UserRegistration.Domain.Models;

namespace UserRegistration.Api.Mappings;

/// <summary>
/// Centralises the conversion between the internal model and the
/// public response DTO. Keeping it in its own file makes the rule
/// "UserResponse never carries Password" explicit and easy to audit.
/// </summary>
public static class UserMappings
{
    public static UserResponse ToResponse(User user) => new(
        user.Id,
        user.Email,
        user.Phone,
        user.AddressLine,
        user.AddressComplement,
        user.City,
        user.State,
        user.ZipCode);
}