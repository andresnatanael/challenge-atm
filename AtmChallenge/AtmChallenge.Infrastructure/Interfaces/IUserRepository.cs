using AtmChallenge.Domain.Entities;

namespace AtmChallenge.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task UpdateAsync(User user);
    Task<User?> GetByUsernameAsync(string username);
}