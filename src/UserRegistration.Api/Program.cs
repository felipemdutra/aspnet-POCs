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
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUserService, UserService>();

// IUserStore is scoped because it depends on the scoped UserDbContext;
// the request lifetime matches the database lifetime.
builder.Services.AddScoped<IUserStore, EfUserStore>();

var app = builder.Build();

// Make sure the SQLite database and schema exist before the first
// request reaches the application. EnsureCreated is appropriate for
// this POC; a production system would use migrations instead.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.EnsureCreated();
}

// Centralised error handling: any unhandled exception becomes a
// ProblemDetails response, and empty status code bodies are filled.
app.UseExceptionHandler();
app.UseStatusCodePages();

// OpenAPI document and Scalar UI are only useful while developing,
// so we gate them behind the Development environment.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// All user routes live behind a single extension method, so this
// file stays focused on wiring and configuration.
app.MapUserEndpoints();

app.Run("http://localhost:5291");