using UserRegistrationPOC.Models;

namespace UserRegistrationPOC.Services;

/// <summary>
/// Contract that any user storage must implement. The endpoints depend
/// only on this abstraction, so swapping InMemoryUserStore for a real
/// database later will not require changes in the endpoint layer.
/// </summary>
public interface IUserStore
{
    /// <summary>Returns every user currently stored.</summary>
    IEnumerable<User> List();

    /// <summary>
    /// Finds a user by id, or returns null when no user has that id.
    /// </summary>
    User? GetById(long id);

    /// <summary>Appends a new user. The id comes from the entity itself.</summary>
    void Add(User user);

    /// <summary>
    /// Removes the user with the given id.
    /// Returns true when something was removed, false when no user matched.
    /// </summary>
    bool Remove(long id);
}
