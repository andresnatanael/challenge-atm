using AtmChallenge.Application.Interfaces;
using AtmChallenge.Domain.Entities;
using AtmChallenge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

namespace AtmChallenge.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        private readonly CardRepository _cardRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(UserRepository userRepository, CardRepository cardRepository, ILogger<UserService> logger
        )
        {
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _logger = logger;
        }
        
        public async Task<User> AuthenticateUserAsync(string encryptedCardNumber, string encryptedPin)
        {
            var card = await _cardRepository.AuthenticateCardAsync(encryptedCardNumber,encryptedPin);
            if (card == null)
            {
                _logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}", encryptedCardNumber);
                throw new Exception("Card not found for encrypted card number: " + encryptedCardNumber);
            }
            return card.User;
        }

        public async Task<bool?> IsCardNumberLockedOutAsync(string encryptedCardNumber)
        {
            var card = await _cardRepository.GetCardByHash(encryptedCardNumber);

            if (card == null)
            {
                _logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}", encryptedCardNumber);
                throw new Exception("Card not found for encrypted card number: " + encryptedCardNumber);
            }

            if (card.FailedLoginAttempts >= 4)
            {
                return true;
            }
            
            return false;
        }

        public async Task RecordFailedLoginAsync(string encryptedCardNumber)
        {
            var card = await _cardRepository.GetCardByHash(encryptedCardNumber);

            if (card == null)
            {
                _logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}", encryptedCardNumber); 
                throw new Exception("Card not found for encrypted card number: " + encryptedCardNumber);
            }
            
            card.FailedLoginAttempts++;

            await _cardRepository.UpdateAsync(card);
        }

        public async Task ResetFailedAttemptsAsync(string encryptedCardNumber)
        {
            var card = await _cardRepository.GetCardByHash(encryptedCardNumber);

            if (card == null)
            {
                _logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}", encryptedCardNumber); 
                throw new Exception("Card not found for encrypted card number: " + encryptedCardNumber);
            }
            
            card.FailedLoginAttempts = 0;

            await _cardRepository.UpdateAsync(card);
        }
    }
}