using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using AtmChallenge.Application.Exceptions;
using AtmChallenge.Application.Interfaces;
using AtmChallenge.Application.Services;
using AtmChallenge.Domain.Entities;
using AtmChallenge.Domain.Entities.Card;
using AtmChallenge.Infrastructure.Interfaces;
using AtmChallenge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AtmChallenge.Tests.Application.Services
{
    public class UserServiceTests
    {
        private readonly Mock<ICardRepository> _mockCardRepository;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _mockCardRepository = new Mock<ICardRepository>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _userService = new UserService(_mockCardRepository.Object, _mockLogger.Object);
        }

        #region AuthenticateUserAsync Tests

        [Fact]
        public async Task AuthenticateUserAsync_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var encryptedPin = "encrypted_pin_456";
            var user = new User { Id = 1, Username = "Test User" };
            var card = new Card
            {
                NumberHash = encryptedCardNumber,
                PinHash = encryptedPin,
                User = user
            };

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);

            // Act
            var result = await _userService.AuthenticateUserAsync(encryptedCardNumber, encryptedPin);

            // Assert
            Assert.Equal(user, result);
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
        }

        [Fact]
        public async Task AuthenticateUserAsync_WithNonExistentCard_ThrowsCardNotFoundException()
        {
            // Arrange
            var encryptedCardNumber = "non_existent_card";
            var encryptedPin = "some_pin";

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync((Card)null);

            // Act & Assert
            await Assert.ThrowsAsync<CardNotFoundException>(() => 
                _userService.AuthenticateUserAsync(encryptedCardNumber, encryptedPin));
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
        }

        [Fact]
        public async Task AuthenticateUserAsync_WithIncorrectPin_ThrowsInvalidCredentialException()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var correctPin = "correct_pin";
            var incorrectPin = "wrong_pin";
            var user = new User { Id = 1, Username = "Test User" };
            var card = new Card
            {
                NumberHash = encryptedCardNumber,
                PinHash = correctPin,
                User = user
            };

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialException>(() => 
                _userService.AuthenticateUserAsync(encryptedCardNumber, incorrectPin));
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
        }

        #endregion

        #region IsCardNumberLockedOutAsync Tests

        [Fact]
        public async Task IsCardNumberLockedOutAsync_WithNonLockedCard_ReturnsFalse()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var card = new Card
            {
                NumberHash = encryptedCardNumber,
                FailedLoginAttempts = 3
            };

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);

            // Act
            var result = await _userService.IsCardNumberLockedOutAsync(encryptedCardNumber);

            // Assert
            Assert.False(result);
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
        }

        [Fact]
        public async Task IsCardNumberLockedOutAsync_WithLockedCard_ReturnsTrue()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var card = new Card
            {
                NumberHash = encryptedCardNumber,
                FailedLoginAttempts = 4
            };

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);

            // Act
            var result = await _userService.IsCardNumberLockedOutAsync(encryptedCardNumber);

            // Assert
            Assert.True(result);
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
        }

        [Fact]
        public async Task IsCardNumberLockedOutAsync_WithNonExistentCard_ThrowsCardNotFoundException()
        {
            // Arrange
            var encryptedCardNumber = "non_existent_card";

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync((Card)null);

            // Act & Assert
            await Assert.ThrowsAsync<CardNotFoundException>(() => 
                _userService.IsCardNumberLockedOutAsync(encryptedCardNumber));
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
        }

        #endregion

        #region RecordFailedLoginAsync Tests

        [Fact]
        public async Task RecordFailedLoginAsync_WithValidCard_IncrementsFailedAttempts()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var initialFailedAttempts = 2;
            var card = new Card
            {
                NumberHash = encryptedCardNumber,
                FailedLoginAttempts = initialFailedAttempts
            };

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);
            
            _mockCardRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Card>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.RecordFailedLoginAsync(encryptedCardNumber);

            // Assert
            Assert.Equal(initialFailedAttempts + 1, card.FailedLoginAttempts);
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
            _mockCardRepository.Verify(repo => repo.UpdateAsync(card), Times.Once);
        }

        [Fact]
        public async Task RecordFailedLoginAsync_WithNonExistentCard_ThrowsCardNotFoundException()
        {
            // Arrange
            var encryptedCardNumber = "non_existent_card";

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync((Card)null);

            // Act & Assert
            await Assert.ThrowsAsync<CardNotFoundException>(() => 
                _userService.RecordFailedLoginAsync(encryptedCardNumber));
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
            _mockCardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }

        #endregion

        #region ResetFailedAttemptsAsync Tests

        [Fact]
        public async Task ResetFailedAttemptsAsync_WithValidCard_ResetsFailedAttempts()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var card = new Card
            {
                NumberHash = encryptedCardNumber,
                FailedLoginAttempts = 3
            };

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);
            
            _mockCardRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Card>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.ResetFailedAttemptsAsync(encryptedCardNumber);

            // Assert
            Assert.Equal(0, card.FailedLoginAttempts);
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
            _mockCardRepository.Verify(repo => repo.UpdateAsync(card), Times.Once);
        }

        [Fact]
        public async Task ResetFailedAttemptsAsync_WithNonExistentCard_ThrowsCardNotFoundException()
        {
            // Arrange
            var encryptedCardNumber = "non_existent_card";

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync((Card)null);

            // Act & Assert
            await Assert.ThrowsAsync<CardNotFoundException>(() => 
                _userService.ResetFailedAttemptsAsync(encryptedCardNumber));
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
            _mockCardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }

        #endregion
    }
}