using AtmChallenge.Domain.Entities;

namespace AtmChallenge.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> AuthenticateUserAsync(string cardNumber, string pin);
        Task<bool> IsCardNumberLockedOutAsync(string cardNumber);
        Task RecordFailedLoginAsync(string cardNumber);
        Task ResetFailedAttemptsAsync(string cardNumber);
    }
}