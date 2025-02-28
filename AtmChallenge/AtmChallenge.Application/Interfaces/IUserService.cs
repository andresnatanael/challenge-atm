using AtmChallenge.Domain.Entities;

namespace AtmChallenge.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<bool> IsUserLockedOutAsync(string username);
        Task RecordFailedLoginAsync(string username);
        Task ResetFailedAttemptsAsync(string username);
    }
}