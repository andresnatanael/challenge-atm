using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtmChallenge.Application.Exceptions;
using AtmChallenge.Application.Interfaces;
using AtmChallenge.Application.Services;
using AtmChallenge.Domain.Entities.Card;
using AtmChallenge.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AtmChallenge.Tests.Application.Services
{
    public class CardServiceTests
    {
        private readonly Mock<ICardRepository> _mockCardRepository;
        private readonly Mock<ILogger<CardService>> _mockLogger;
        private readonly CardService _cardService;

        public CardServiceTests()
        {
            _mockCardRepository = new Mock<ICardRepository>();
            _mockLogger = new Mock<ILogger<CardService>>();
            _cardService = new CardService(_mockCardRepository.Object, _mockLogger.Object);
        }

        #region GetBalance Tests

        [Fact]
        public async Task GetBalance_WithValidCardNumber_ReturnsCorrectBalance()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var expectedBalance = 1000.50;
            var card = new Card { NumberHash = encryptedCardNumber, Balance = expectedBalance };

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);

            // Act
            var result = await _cardService.GetBalance(encryptedCardNumber);

            // Assert
            Assert.Equal(expectedBalance, result);
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
        }

        [Fact]
        public async Task GetBalance_WithNonExistentCard_ThrowsCardNotFoundException()
        {
            // Arrange
            var encryptedCardNumber = "non_existent_card";

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync((Card)null);

            // Act & Assert
            await Assert.ThrowsAsync<CardNotFoundException>(() => 
                _cardService.GetBalance(encryptedCardNumber));
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
        }

        #endregion

        #region GetTransactions Tests

        [Fact]
        public async Task GetTransactions_ReturnsCorrectPaginatedTransactions()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var page = 1;
            var pageSize = 10;
            var expectedTransactions = new List<CardTransaction>
            {
                new CardTransaction { Id = 1, Amount = -50.0, Type = TransactionType.Withdrawal, Date = DateTime.UtcNow.AddDays(-1) },
                new CardTransaction { Id = 2, Amount = 100.0, Type = TransactionType.Deposit, Date = DateTime.UtcNow.AddDays(-2) }
            };
            var expectedTotalCount = 5;

            _mockCardRepository.Setup(repo => repo.GetPaginatedTransactionsAsync(encryptedCardNumber, page, pageSize))
                .ReturnsAsync((expectedTransactions, expectedTotalCount));

            // Act
            var (transactions, totalCount) = await _cardService.GetTransactions(encryptedCardNumber, page, pageSize);

            // Assert
            Assert.Equal(expectedTransactions, transactions);
            Assert.Equal(expectedTotalCount, totalCount);
            _mockCardRepository.Verify(repo => repo.GetPaginatedTransactionsAsync(encryptedCardNumber, page, pageSize), Times.Once);
        }

        #endregion

        #region Withdraw Tests

        [Fact]
        public async Task Withdraw_WithValidParams_SuccessfullyWithdraws()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var initialBalance = 1000.0;
            var withdrawAmount = 100.0;
            var expectedBalance = initialBalance - withdrawAmount;
            var idempotencyKey = Guid.NewGuid().ToString();
            
            var card = new Card { NumberHash = encryptedCardNumber, Balance = initialBalance };
            
            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);
            
            _mockCardRepository.Setup(repo => repo.GetCardTransactionByIdempotencyKey(idempotencyKey))
                .ReturnsAsync((CardTransaction)null);
                
            _mockCardRepository.Setup(repo => repo.AddTransactionAsync(It.IsAny<CardTransaction>()))
                .Returns(Task.CompletedTask);
                
            _mockCardRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Card>()))
                .Returns(Task.CompletedTask);

            // Act
            var (transaction, newBalance) = await _cardService.Withdraw(encryptedCardNumber, withdrawAmount, idempotencyKey);

            // Assert
            Assert.Equal(-withdrawAmount, transaction.Amount);
            Assert.Equal(TransactionType.Withdrawal, transaction.Type);
            Assert.Equal(idempotencyKey, transaction.IdempotencyKey);
            Assert.Equal(expectedBalance, newBalance);
            Assert.Equal(card, transaction.Card);
            Assert.Equal("Ezeiza Airport", transaction.AtmLocation);
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
            _mockCardRepository.Verify(repo => repo.GetCardTransactionByIdempotencyKey(idempotencyKey), Times.Once);
            _mockCardRepository.Verify(repo => repo.AddTransactionAsync(It.IsAny<CardTransaction>()), Times.Once);
            _mockCardRepository.Verify(repo => repo.UpdateAsync(card), Times.Once);
        }

        [Fact]
        public async Task Withdraw_WithNonExistentCard_ThrowsCardNotFoundException()
        {
            // Arrange
            var encryptedCardNumber = "non_existent_card";
            var withdrawAmount = 100.0;
            var idempotencyKey = Guid.NewGuid().ToString();

            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync((Card)null);

            // Act & Assert
            await Assert.ThrowsAsync<CardNotFoundException>(() => 
                _cardService.Withdraw(encryptedCardNumber, withdrawAmount, idempotencyKey));
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
            _mockCardRepository.Verify(repo => repo.GetCardTransactionByIdempotencyKey(idempotencyKey), Times.Never);
            _mockCardRepository.Verify(repo => repo.AddTransactionAsync(It.IsAny<CardTransaction>()), Times.Never);
            _mockCardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }

        [Fact]
        public async Task Withdraw_WithDuplicateIdempotencyKey_ThrowsTxDuplicatedException()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var withdrawAmount = 100.0;
            var idempotencyKey = "existing_key";
            var existingTransaction = new CardTransaction 
            { 
                Id = 123, 
                IdempotencyKey = idempotencyKey, 
                Amount = -withdrawAmount 
            };
            
            var card = new Card { NumberHash = encryptedCardNumber, Balance = 1000.0 };
            
            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);
                
            _mockCardRepository.Setup(repo => repo.GetCardTransactionByIdempotencyKey(idempotencyKey))
                .ReturnsAsync(existingTransaction);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TxDuplicatedException>(() => 
                _cardService.Withdraw(encryptedCardNumber, withdrawAmount, idempotencyKey));
                
            Assert.Contains(existingTransaction.Id.ToString(), exception.Message);
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
            _mockCardRepository.Verify(repo => repo.GetCardTransactionByIdempotencyKey(idempotencyKey), Times.Once);
            _mockCardRepository.Verify(repo => repo.AddTransactionAsync(It.IsAny<CardTransaction>()), Times.Never);
            _mockCardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }

        [Fact]
        public async Task Withdraw_WithInsufficientBalance_ThrowsInsufficientBalanceException()
        {
            // Arrange
            var encryptedCardNumber = "encrypted_card_123";
            var balance = 50.0;
            var withdrawAmount = 100.0; // More than balance
            var idempotencyKey = Guid.NewGuid().ToString();
            
            var card = new Card { NumberHash = encryptedCardNumber, Balance = balance };
            
            _mockCardRepository.Setup(repo => repo.GetCardByHash(encryptedCardNumber))
                .ReturnsAsync(card);
                
            _mockCardRepository.Setup(repo => repo.GetCardTransactionByIdempotencyKey(idempotencyKey))
                .ReturnsAsync((CardTransaction)null);

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientBalanceException>(() => 
                _cardService.Withdraw(encryptedCardNumber, withdrawAmount, idempotencyKey));
            
            _mockCardRepository.Verify(repo => repo.GetCardByHash(encryptedCardNumber), Times.Once);
            _mockCardRepository.Verify(repo => repo.GetCardTransactionByIdempotencyKey(idempotencyKey), Times.Once);
            _mockCardRepository.Verify(repo => repo.AddTransactionAsync(It.IsAny<CardTransaction>()), Times.Never);
            _mockCardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }

        #endregion
    }
}