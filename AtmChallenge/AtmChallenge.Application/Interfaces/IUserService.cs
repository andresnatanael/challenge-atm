using AtmChallenge.Domain.Entities;
using AtmChallenge.Domain.Entities.Card;

namespace AtmChallenge.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> AuthenticateUserAsync(string encryptedCardNumber, string encryptedPin);
        Task<bool?> IsCardNumberLockedOutAsync(string encryptedCardNumber);
        Task RecordFailedLoginAsync(string encryptedCardNumber);
        Task ResetFailedAttemptsAsync(string encryptedCardNumber);
    }
}