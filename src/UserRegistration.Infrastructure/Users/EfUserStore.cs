using Microsoft.EntityFrameworkCore;
using UserRegistration.Application.Abstractions;
using UserRegistration.Domain.Models;

namespace UserRegistration.Infrastructure.Users;

public sealed class EfUserStore : IUserStore
{
    private readonly UserDbContext _db;

    public EfUserStore(UserDbContext db) => _db = db;

    public IEnumerable<User> List() =>
        _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToList();

    public User? GetById(long id) =>
        _db.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == id);

    public void Add(User user)
    {
        _db.Users.Add(user);
        _db.SaveChanges();
    }

    public bool Remove(long id)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return false;
        }

        _db.Users.Remove(user);
        _db.SaveChanges();
        return true;
    }

    public bool Update(User user)
    {
        var existing = _db.Users.FirstOrDefault(u => u.Id == user.Id);
        if (existing is null)
        {
            return false;
        }

        // Attach the incoming aggregate to the change tracker so EF
        // marks its properties as modified. We copy the new values
        // onto the tracked instance because the incoming `user` came
        // from the application layer with no change tracking.
        _db.Entry(existing).CurrentValues.SetValues(user);
        _db.SaveChanges();
        return true;
    }

    public User? GetByKeycloakSub(string keycloakSub) =>
        _db.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.KeycloakSub == keycloakSub);
}