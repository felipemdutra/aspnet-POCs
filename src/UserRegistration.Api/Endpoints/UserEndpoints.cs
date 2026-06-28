using Microsoft.AspNetCore.Mvc;
using UserRegistration.Api.Mappings;
using UserRegistration.Api.Requests;
using UserRegistration.Api.Responses;
using UserRegistration.Application.Abstractions;
using UserRegistration.Domain.Models;

namespace UserRegistration.Api.Endpoints;

/// <summary>
/// Hosts every HTTP route related to users. Exposed as an extension
/// method so Program.cs only needs to call app.MapUserEndpoints()
/// without knowing the individual routes.
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // MapGroup lets us share the "/users" prefix and common
        // configuration across all user routes.
        var group = app.MapGroup("/users");

        group.MapPost("/", ([FromBody] CreateUserRequest request, IUserStore store) =>
        {
            // Build the entity through the factory so invariants are
            // enforced even if a future caller bypasses the DTO.
            var user = User.Create(
                request.Email,
                request.Password,
                request.Phone,
                request.AddressLine,
                request.AddressComplement,
                request.City,
                request.State,
                request.ZipCode);

            store.Add(user);

            return Results.Created($"/users/{user.Id}", UserMappings.ToResponse(user));
        })
        .WithName("CreateUser")
        .WithSummary("Creates a new user")
        .WithDescription(
            "Registers a new user with the provided account and address details. " +
            "Returns the created user resource along with its location URI.")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:long}", (long id, IUserStore store) =>
        {
            var user = store.GetById(id);
            return user is null
                ? Results.NotFound()
                : Results.Ok(UserMappings.ToResponse(user));
        })
        .WithName("GetUserById")
        .WithSummary("Gets a user by ID")
        .WithDescription(
            "Retrieves the details of a single user identified by their numeric ID. " +
            "Returns 404 if no user with the given ID exists.")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", (IUserStore store) =>
        {
            return Results.Ok(store.List().Select(UserMappings.ToResponse));
        })
        .WithName("GetUsers")
        .WithSummary("Gets all users")
        .WithDescription("Retrieves the full list of registered users.")
        .Produces<IEnumerable<UserResponse>>(StatusCodes.Status200OK);

        group.MapPut("/{id:long}", (long id, [FromBody] UpdateUserRequest request, IUserStore store) =>
        {
            var user = store.GetById(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            // Behavioural methods on User keep validation and intent
            // together instead of poking private fields from outside.
            user.ChangeEmail(request.Email);
            user.ChangePassword(request.Password);
            user.ChangePhone(request.Phone);
            user.ChangeAddress(
                request.AddressLine,
                request.AddressComplement,
                request.City,
                request.State,
                request.ZipCode);

            return Results.Ok(UserMappings.ToResponse(user));
        })
        .WithName("UpdateUser")
        .WithSummary("Updates an existing user")
        .WithDescription(
            "Replaces the account and address details of the user identified by the " +
            "given ID with the values provided in the request body. " +
            "Returns 404 if no user with the given ID exists.")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:long}", (long id, IUserStore store) =>
        {
            return store.Remove(id)
                ? Results.NoContent()
                : Results.NotFound();
        })
        .WithName("DeleteUser")
        .WithSummary("Deletes a user")
        .WithDescription(
            "Permanently removes the user identified by the given ID. " +
            "Returns 404 if no user with the given ID exists, or 204 if the deletion succeeds.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}