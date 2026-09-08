using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserRegistration.Api.Endpoints;
using UserRegistration.Application.Users;
using UserRegistration.Application.Abstractions;
using UserRegistration.Infrastructure.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing 'ConnectionStrings:Default' in appsettings.json");

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserService, UserService>();

// IUserStore is scoped because it depends on the scoped UserDbContext;
// the request lifetime matches the database lifetime.
builder.Services.AddScoped<IUserStore, EfUserStore>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}

// Centralised error handling: any unhandled exception becomes a
// ProblemDetails response, and empty status code bodies are filled.
app.UseExceptionHandler();
app.UseStatusCodePages();

bool isDev = false;

// OpenAPI document and Scalar UI are only useful while developing,
// so we gate them behind the Development environment.
if (app.Environment.IsDevelopment())
{
    isDev = true;
    app.MapOpenApi();
    app.MapScalarApiReference();
}

Console.WriteLine($"Is development: {isDev}");

// All user routes live behind a single extension method, so this
// file stays focused on wiring and configuration.
app.MapUserEndpoints();

// no argument lets ASPNETCORE_URLS take over so no hardcoding.
app.Run();