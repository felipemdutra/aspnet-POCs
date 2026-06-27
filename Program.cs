using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

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
})
.WithName("CreateUser")
.WithSummary("Creates a new user")
.WithDescription("Registers a new user with the provided account and address details. Returns the created user resource along with its location URI.")
.Produces<UserResponse>(StatusCodes.Status201Created)
.ProducesValidationProblem(StatusCodes.Status400BadRequest);

app.MapGet("/users/{id:long}", (long id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user == null ? Results.NotFound() : Results.Ok(UserResponse.From(user));
})
.WithName("GetUserById")
.WithSummary("Gets a user by ID.")
.WithDescription("Retrieves the details of a single user identified by their numeric ID. Returns 404 if no user with the given ID exists.")
.Produces<UserResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("/users", () =>
{
    return Results.Ok(users.Select(UserResponse.From));
})
.WithName("GetUsers")
.WithSummary("Gets all users")
.WithDescription("Retrieves the full list of registered users.")
.Produces<IEnumerable<UserResponse>>(StatusCodes.Status200OK);

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
})
.WithName("UpdateUser")
.WithSummary("Updates an existing user")
.WithDescription("Replaces the account and address details of the user identified by the given ID with the values provided in the request body. Returns 404 if no user with the given ID exists")
.Produces<UserResponse>(StatusCodes.Status200OK)
.ProducesValidationProblem(StatusCodes.Status400BadRequest);

app.MapDelete("/users/{id:long}", (long id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        return Results.NotFound();
    }

    users.Remove(user);

    return Results.NoContent();
})
.WithName("Delete user")
.WithSummary("Deletes a user")
.WithDescription("Permanently removes the user identified by the given ID. Returns 404 if no user with the given ID exists, or 204 if the deletion succeeds.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

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
