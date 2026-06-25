using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();

var app = builder.Build();

var users = new List<User>
{
    new("john@gmail.com", "secretpassword123", "+5511999999999", "123 Main St", null, "Sao Paulo", "SP", "01310-100"),
    new("mary@hotmail.com", "123321aa", "+5511988888888", "456 Oak Ave", "Apt 12", "Rio de Janeiro", "RJ", "20040-020")
};

app.MapPost("/users", ([FromBody] CreateUserRequest createUserDto) =>
{
    var user = new User(
        createUserDto.Email,
        createUserDto.Password,
        createUserDto.Phone,
        createUserDto.AddressLine,
        createUserDto.AddressComplement,
        createUserDto.City,
        createUserDto.State,
        createUserDto.ZipCode);
    users.Add(user);

    return Results.Created($"/users/{user.Id}", UserResponse.From(user));
});

app.MapGet("/users/{id:long}", (long id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user == null ? Results.NotFound() : Results.Ok(UserResponse.From(user));
});

app.MapGet("/users", () =>
{
    return Results.Ok(users.Select(UserResponse.From));
});

app.MapPut("/users/{id:long}", (long id, [FromBody] UpdateUserRequest newUser) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);

    if (user == null)
    {
        return Results.NotFound();
    }

    user.Email = newUser.Email;
    user.Password = newUser.Password;
    user.Phone = newUser.Phone;
    user.AddressLine = newUser.AddressLine;
    user.AddressComplement = newUser.AddressComplement;
    user.City = newUser.City;
    user.State = newUser.State;
    user.ZipCode = newUser.ZipCode;

    return Results.Ok(UserResponse.From(user));
});

app.MapDelete("/users/{id:long}", (long id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        return Results.NotFound();
    }

    users.Remove(user);

    return Results.NoContent();
});

app.Run("http://localhost:5291");

public class User
{
    private static long nextId = 0;

    public string Email { get; set; }
    public string Password { get; set; }
    public string? Phone { get; set; }
    public string AddressLine { get; set; }
    public string? AddressComplement { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public long Id { get; }

    public User(
        string email,
        string password,
        string? phone,
        string addressLine,
        string? addressComplement,
        string city,
        string state,
        string zipCode)
    {
        Email = email;
        Password = password;
        Phone = phone;
        AddressLine = addressLine;
        AddressComplement = addressComplement;
        City = city;
        State = state;
        ZipCode = zipCode;
        Id = nextId++;
    }
}

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

public sealed class UpdateUserRequest
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

public sealed record UserResponse(
    long Id,
    string Email,
    string? Phone,
    string AddressLine,
    string? AddressComplement,
    string City,
    string State,
    string ZipCode)
{
    public static UserResponse From(User user) => new(
        user.Id,
        user.Email,
        user.Phone,
        user.AddressLine,
        user.AddressComplement,
        user.City,
        user.State,
        user.ZipCode);
}
