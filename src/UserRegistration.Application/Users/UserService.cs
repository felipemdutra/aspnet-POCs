using UserRegistration.Application.Abstractions;
using UserRegistration.Domain.Models;

namespace UserRegistration.Application.Users;

public class UserService : IUserService
{
    private readonly IUserStore _store;

    public UserService(IUserStore store) => _store = store;
    
    public User? GetById(long id) => _store.GetById(id);

    public User Create(UserInput input)
    {
        var user = User.Create(
            input.Email,
            input.Password,
            input.Phone,
            input.AddressLine,
            input.AddressComplement,
            input.City,
            input.State,
            input.ZipCode);

        _store.Add(user);
        return user;
    }

    public IEnumerable<User> List() => _store.List();

    public User? Update(long id, UserInput input)
    {
        User? existing = _store.GetById(id);
        if (existing is null)
        {
            return null;
        }

        existing.ChangeEmail(input.Email);
        existing.ChangePassword(input.Password);
        existing.ChangeAddress(input.AddressLine, input.AddressComplement, input.City, input.State, input.ZipCode);
        existing.ChangePhone(input.Phone);

        return _store.Update(existing) ? existing : null;
    }

    public bool Delete(long id) => _store.Remove(id);
}
