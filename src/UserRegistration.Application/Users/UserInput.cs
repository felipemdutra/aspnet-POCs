namespace UserRegistration.Application.Users;

public sealed record UserInput(
    string Email,
    string Password,
    string? Phone,
    string AddressLine,
    string? AddressComplement,
    string City,
    string State,
    string ZipCode);
