using System.ComponentModel.DataAnnotations;

namespace UserRegistrationPOC.Requests;

/// <summary>
/// DTO used as the body of POST /users.
/// Contains only the fields the client is allowed to send when creating
/// a new user account. Password is accepted here on purpose but never
/// returned in any response.
/// </summary>
public sealed class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? Phone { get; init; }

    [Required]
    [StringLength(200)]
    public string AddressLine { get; init; } = string.Empty;

    [StringLength(100)]
    public string? AddressComplement { get; init; }

    [Required]
    [StringLength(100)]
    public string City { get; init; } = string.Empty;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string State { get; init; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{5}-?\d{3}$")]
    public string ZipCode { get; init; } = string.Empty;
}
