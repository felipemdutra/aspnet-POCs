namespace UserRegistration.Application.Users;

public sealed record UserInput(
    string KeycloakSub,
    string Email,
    string? Phone,
    string AddressLine,
    string? AddressComplement,
    string City,
    string State,
    string ZipCode);
