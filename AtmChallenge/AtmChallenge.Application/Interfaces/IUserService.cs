using AtmChallenge.Domain.Entities;
using AtmChallenge.Domain.Entities.Card;

namespace AtmChallenge.Application.Interfaces
{
    public interface IUserService
    {
        Task<string> AuthenticateUserAsync(string encryptedCardNumber, string encryptedPin);
        Task<Card?> IsCardNumberLockedOutAsync(string encryptedCardNumber);
        Task RecordFailedLoginAsync(string encryptedCardNumber);
        Task ResetFailedAttemptsAsync(string encryptedCardNumber);
    }
}