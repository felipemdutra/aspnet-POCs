using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var users = new List<User>
{
    new("john@gmail.com", "secretpassword123"),
    new("mary@hotmail.com", "123321")
};

app.MapPost("/users", ([FromBody] User user) =>
{
    users.Add(user);
    return Results.Created($"users/{user.Id}", user);
});

app.MapGet("/users/{id:long}", (long id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user == null ? Results.NotFound() : Results.Ok(user);
});

app.MapGet("/users", () =>
{
    return Results.Ok(users);
});

app.MapPut("users/{id:long}", (long id, [FromBody] User newUser) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);

    if (user == null)
    {
        return Results.NotFound();
    }

    user.Email = newUser.Email;
    user.Password = newUser.Password;

    return Results.Ok(user);
});

app.MapDelete("users/{id:long}", (long id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        return Results.NotFound();
    }

    users.Remove(user);

    return Results.Ok("User deleted successfully");
});

app.Run("http://localhost:5291");

class User
{
    private static long nextId = 0;

    public string Email { get; set; }
    public string Password { get; set; }
    public long Id { get; }

    public User(string email, string password)
    {
        Password = password;
        Email = email;
        Id = nextId++;
    }
}
