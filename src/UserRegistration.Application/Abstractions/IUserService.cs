using UserRegistration.Domain.Models;
using UserRegistration.Application.Users;

namespace UserRegistration.Application.Abstractions;

public interface IUserService
{
    User Create(UserInput input);
    IEnumerable<User> List();
    User? Update(long id, UserInput input);
    User? GetById(long id);
    bool Delete(long id);
}
