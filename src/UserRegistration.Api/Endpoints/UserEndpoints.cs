using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using UserRegistration.Api.Mappings;
using UserRegistration.Api.Requests;
using UserRegistration.Api.Responses;
using UserRegistration.Application.Abstractions;
using UserRegistration.Application.Users;

namespace UserRegistration.Api.Endpoints;

/// <summary>
/// Hosts every HTTP route related to the caller's user record.
/// Identity (username, password, OTP, session) lives in Keycloak; this
/// API only owns the user data and identifies the caller by the
/// "sub" claim issued by Keycloak. Exposed as an extension method so
/// Program.cs only needs to call app.MapUserEndpoints() without knowing
/// the individual routes.
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // MapGroup lets us share the "/users" prefix and a single
        // authorization gate across every route. RequireAuthorization
        // is what forces the JwtBearer middleware to validate the
        // access token on every request, populating HttpContext.User
        // with the "sub" claim we use as the user key.
        var group = app.MapGroup("/users").RequireAuthorization();

        group.MapGet("/", (ClaimsPrincipal user, IUserService service) =>
        {
            var sub = user.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
            {
                // The token passed signature/issuer/audience/lifetime
                // checks but still didn't carry a "sub". Treat it as
                // an authentication failure rather than a 500.
                return Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Missing 'sub' claim");
            }

            var userRecord = service.GetByKeycloakSub(sub);
            return userRecord is null
                ? Results.NotFound()
                : Results.Ok(UserMappings.ToResponse(userRecord));
        })
        .WithName("GetUser")
        .WithSummary("Gets the caller's user")
        .WithDescription(
            "Returns the user (email, phone, address) linked to the " +
            "authenticated caller's Keycloak 'sub'. Returns 404 if no user has " +
            "been created yet.")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/", (
            [FromBody] UpdateUserRequest request,
            ClaimsPrincipal user,
            IUserService service) =>
        {
            var sub = user.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Missing 'sub' claim");
            }

            // The "sub" comes from the validated access token, never
            // from the request body. That keeps the caller from
            // impersonating another user by sending a different value.
            var input = new UserInput(
                KeycloakSub: sub,
                Email:       request.Email,
                Phone:       request.Phone,
                AddressLine: request.AddressLine,
                AddressComplement: request.AddressComplement,
                City:        request.City,
                State:       request.State,
                ZipCode:     request.ZipCode);

            var userRecord = service.Upsert(input);
            return Results.Ok(UserMappings.ToResponse(userRecord));
        })
        .WithName("UpsertUser")
        .WithSummary("Creates or updates the caller's user")
        .WithDescription(
            "Upserts the user (email, phone, address) linked to the " +
            "authenticated caller's Keycloak 'sub'. The 'sub' is taken from the " +
            "validated access token, not from the request body, so the caller " +
            "cannot create or modify a user that belongs to someone else.")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}