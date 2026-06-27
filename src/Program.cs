using Scalar.AspNetCore;
using UserRegistrationPOC.Endpoints;
using UserRegistrationPOC.Services;

var builder = WebApplication.CreateBuilder(args);

// Framework-level services: validation of DTOs, OpenAPI document,
// and ProblemDetails for predictable error responses.
builder.Services.AddValidation();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Application services. We register IUserStore as a singleton because
// InMemoryUserStore holds state in a private list for the whole
// process lifetime.
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();

var app = builder.Build();

// Centralised error handling: any unhandled exception becomes a
// ProblemDetails response, and empty status code bodies are filled.
app.UseExceptionHandler();
app.UseStatusCodePages();

Console.WriteLine("HI!");

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
