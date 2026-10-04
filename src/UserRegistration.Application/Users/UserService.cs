using UserRegistration.Application.Abstractions;
using UserRegistration.Domain.Models;

namespace UserRegistration.Application.Users;

public class UserService : IUserService
{
    private readonly IUserStore _store;

    public UserService(IUserStore store) => _store = store;

    public User? GetByKeycloakSub(string keycloakSub) =>
        _store.GetByKeycloakSub(keycloakSub);

    public User Upsert(UserInput input)
    {
        var existing = _store.GetByKeycloakSub(input.KeycloakSub);
        if (existing is null)
        {
            var created = User.Create(
                input.KeycloakSub,
                input.Email,
                input.Phone,
                input.AddressLine,
                input.AddressComplement,
                input.City,
                input.State,
                input.ZipCode);

            _store.Add(created);
            return created;
        }

        existing.ChangeEmail(input.Email);
        existing.ChangePhone(input.Phone);
        existing.ChangeAddress(
            input.AddressLine,
            input.AddressComplement,
            input.City,
            input.State,
            input.ZipCode);

        _store.Update(existing);
        return existing;
    }
}
