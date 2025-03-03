using AtmChallenge.Application.Interfaces;
using AtmChallenge.Domain.Entities;
using AtmChallenge.Infrastructure.Repositories;
using System.Security.Cryptography;
using System.Text;
using AtmChallenge.Domain.Entities.Card;
using Microsoft.Extensions.Logging;

namespace AtmChallenge.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        private readonly CardRepository _cardRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(UserRepository userRepository, CardRepository cardRepository)
        {
            _userRepository = userRepository;
            _cardRepository = cardRepository;
        }


        public Task<string> AuthenticateUserAsync(string encryptedCardNumber, string encryptedPin)
        {
            throw new NotImplementedException();
        }

        public async Task<Card?> IsCardNumberLockedOutAsync(string encryptedCardNumber)
        {
            var card = await _cardRepository.GetCardByHash(encryptedCardNumber);

            if (card == null)
            {
                _logger.LogWarning("Card not found for encrypted card number: {EncryptedCardNumber}", encryptedCardNumber);
                return null;
            }
            
            throw new NotImplementedException();
        }

        public Task RecordFailedLoginAsync(string encryptedCardNumber)
        {
            throw new NotImplementedException();
        }

        public Task ResetFailedAttemptsAsync(string encryptedCardNumber)
        {
            throw new NotImplementedException();
        }
    }
}