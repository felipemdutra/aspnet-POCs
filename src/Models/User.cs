using System.Text.RegularExpressions;

namespace UserRegistrationPOC.Models;

/// <summary>
/// Rich domain model for a registered user.
/// Holds identity, contact and address data and protects its own invariants
/// so no other layer can put the entity into an invalid state.
/// </summary>
public sealed class User
{
    private static long nextId;

    private static readonly Regex ZipCodePattern =
        new(@"^\d{5}-?\d{3}$", RegexOptions.Compiled);

    private static readonly Regex EmailPattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public long Id { get; }

    public string Email { get; private set; }

    public string Password { get; private set; }

    public string? Phone { get; private set; }

    public string AddressLine { get; private set; }

    public string? AddressComplement { get; private set; }

    public string City { get; private set; }

    public string State { get; private set; }

    public string ZipCode { get; private set; }

    // The constructor is private on purpose: outside code can only obtain
    // a valid User through the factory method below, which performs all
    // invariant checks first.
    private User(
        long id,
        string email,
        string password,
        string? phone,
        string addressLine,
        string? addressComplement,
        string city,
        string state,
        string zipCode)
    {
        Id = id;
        Email = email;
        Password = password;
        Phone = phone;
        AddressLine = addressLine;
        AddressComplement = addressComplement;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    /// <summary>
    /// Factory method. Builds a User only after validating every required
    /// invariant. Throws <see cref="ArgumentException"/> if any value is
    /// invalid, so the entity can never exist in an invalid state.
    /// </summary>
    public static User Create(
        string email,
        string password,
        string? phone,
        string addressLine,
        string? addressComplement,
        string city,
        string state,
        string zipCode)
    {
        ValidateEmail(email);
        ValidatePassword(password);
        ValidatePhone(phone);
        ValidateAddressLine(addressLine);
        ValidateCity(city);
        ValidateState(state);
        ValidateZipCode(zipCode);

        return new User(
            nextId++,
            email,
            password,
            phone,
            addressLine,
            addressComplement,
            city,
            state,
            zipCode);
    }

    public void ChangeEmail(string email)
    {
        ValidateEmail(email);
        Email = email;
    }

    public void ChangePassword(string password)
    {
        ValidatePassword(password);
        Password = password;
    }

    public void ChangePhone(string? phone)
    {
        ValidatePhone(phone);
        Phone = phone;
    }

    public void ChangeAddress(
        string addressLine,
        string? complement,
        string city,
        string state,
        string zipCode)
    {
        ValidateAddressLine(addressLine);
        ValidateCity(city);
        ValidateState(state);
        ValidateZipCode(zipCode);

        AddressLine = addressLine;
        AddressComplement = complement;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    // Guard clauses: short, early-return checks that reject invalid input
    // before the entity can absorb it.
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (!EmailPattern.IsMatch(email))
        {
            throw new ArgumentException("Email format is invalid.", nameof(email));
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new ArgumentException(
                "Password must be at least 8 characters long.",
                nameof(password));
        }
    }

    private static void ValidatePhone(string? phone)
    {
        if (phone is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 8)
        {
            throw new ArgumentException("Phone format is invalid.", nameof(phone));
        }
    }

    private static void ValidateAddressLine(string addressLine)
    {
        if (string.IsNullOrWhiteSpace(addressLine))
        {
            throw new ArgumentException("Address is required.", nameof(addressLine));
        }
    }

    private static void ValidateCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City is required.", nameof(city));
        }
    }

    private static void ValidateState(string state)
    {
        if (string.IsNullOrWhiteSpace(state) || state.Length != 2)
        {
            throw new ArgumentException(
                "State must have exactly 2 characters.",
                nameof(state));
        }
    }

    private static void ValidateZipCode(string zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode) || !ZipCodePattern.IsMatch(zipCode))
        {
            throw new ArgumentException(
                "Zip code must match the format 12345-678 or 12345678.",
                nameof(zipCode));
        }
    }
}
