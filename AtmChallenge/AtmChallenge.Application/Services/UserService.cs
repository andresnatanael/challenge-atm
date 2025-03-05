using System.Security.Authentication;
using AtmChallenge.Application.Exceptions;
using AtmChallenge.Application.Interfaces;
using AtmChallenge.Domain.Entities;
using AtmChallenge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AtmChallenge.Application.Services
{
    public class UserService(CardRepository cardRepository, ILogger<UserService> logger) : IUserService
    {
        public async Task<User> AuthenticateUserAsync(string encryptedCardNumber, string encryptedPin)
        {
            var card = await cardRepository.GetCardByHash(encryptedCardNumber);
            if (card == null)
            {
                logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}",
                    encryptedCardNumber);
                throw new CardNotFoundException("Card not found!");
            }
            if (card.PinHash != encryptedPin)
            {
                logger.LogWarning("Incorrect PIN for card number: {EncryptedCardNumber}", encryptedCardNumber);
                throw new InvalidCredentialException();
            }
            return card.User;
        }

        public async Task<bool?> IsCardNumberLockedOutAsync(string encryptedCardNumber)
        {
            var card = await cardRepository.GetCardByHash(encryptedCardNumber);

            if (card == null)
            {
                logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}",
                    encryptedCardNumber);
                throw new CardNotFoundException("Card not found!");
            }
            if (card.FailedLoginAttempts >= 4)
            {
                return true;
            }
            return false;
        }

        public async Task RecordFailedLoginAsync(string encryptedCardNumber)
        {
            var card = await cardRepository.GetCardByHash(encryptedCardNumber);

            if (card == null)
            {
                logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}",
                    encryptedCardNumber);
                throw new CardNotFoundException("Card not found!");
            }

            card.FailedLoginAttempts++;

            await cardRepository.UpdateAsync(card);
        }

        public async Task ResetFailedAttemptsAsync(string encryptedCardNumber)
        {
            var card = await cardRepository.GetCardByHash(encryptedCardNumber);

            if (card == null)
            {
                logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}",
                    encryptedCardNumber);
                throw new CardNotFoundException("Card not found!");
            }

            card.FailedLoginAttempts = 0;

            await cardRepository.UpdateAsync(card);
        }
    }
}