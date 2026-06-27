using UserRegistrationPOC.Models;

namespace UserRegistrationPOC.Services;

/// <summary>
/// In-memory user storage. This is fine in this step of the POC.
/// </summary>
public sealed class InMemoryUserStore : IUserStore
{
    private readonly List<User> users = new()
    {
        User.Create(
            "john@gmail.com",
            "secretpassword123",
            "+5511999999999",
            "123 Main St",
            null,
            "Sao Paulo",
            "SP",
            "01310-100"),
        User.Create(
            "mary@hotmail.com",
            "123321aa",
            "+5511988888888",
            "456 Oak Ave",
            "Apt 12",
            "Rio de Janeiro",
            "RJ",
            "20040-020")
    };

    public IEnumerable<User> List() => users;

    public User? GetById(long id) => users.FirstOrDefault(u => u.Id == id);

    public void Add(User user) => users.Add(user);

    public bool Remove(long id)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return false;
        }

        users.Remove(user);
        return true;
    }
}
