namespace UserRegistration.Api.Responses;

/// <summary>
/// DTO returned by every user endpoint. Notice that Password is NOT
/// included: this is the safe shape we send to clients.
/// </summary>
public sealed record UserResponse(
    long Id,
    string Email,
    string? Phone,
    string AddressLine,
    string? AddressComplement,
    string City,
    string State,
    string ZipCode);