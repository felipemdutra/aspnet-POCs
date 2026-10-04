using UserRegistration.Domain.Models;
using UserRegistration.Application.Users;

namespace UserRegistration.Application.Abstractions;

public interface IUserService
{
    User? GetByKeycloakSub(string keycloakSub);

    // Creates the user if no row exists for the input's Keycloak
    // subject, otherwise updates the existing row with the new field
    // values. The Keycloak subject is never read from client input;
    // the API stamps it in from the validated access token.
    User Upsert(UserInput input);
}
