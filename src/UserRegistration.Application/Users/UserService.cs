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
        // I'm not sure if this approach is the best/correct one. Currently
        // we call Change* twice on the same user instance. Here, and in
        // _store.Update().
        //
        // This happens because _store.Update() takes a User instance to update
        // the user that exists in the store. Creating a new user with the
        // updated fields doesn't work because of the static ID assigned in
        // their creation, which would create "gaps" between users, a
        // disadvantage to our current in-memory store architecture.
        //
        // A solution is to allow _store.Update() to take UserInput as argument
        // instead of a whole user. Which I don't think can be done, as to
        // not "leak" responsabilities between layers, a key consideration to
        // this POC.
        //
        // I understand the current approach might be sloppy, and I
        // fully expect it to be a topic in the next review/POC.

        User? existing = _store.GetById(id);
        if (existing is null)
        {
            return null;
        }

        existing.ChangeEmail(input.Email);
        existing.ChangePassword(input.Password);
        existing.ChangeAddress(input.AddressLine, input.AddressComplement, input.City, input.State, input.ZipCode);
        existing.ChangePhone(input.Phone);

        return _store.Update(id, existing) ? existing : null;
    }

    public bool Delete(long id) => _store.Remove(id);
}
