using AtmChallenge.Application.Exceptions;
using AtmChallenge.Application.Interfaces;
using AtmChallenge.Domain.Entities.Card;
using AtmChallenge.Infrastructure.Interfaces;
using AtmChallenge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AtmChallenge.Application.Services;

public class CardService(ICardRepository cardRepository, ILogger<CardService> logger) : ICardService
{
    public async Task<double> GetBalance(string encryptedCardNumber)
    {
        var card = await cardRepository.GetCardByHash(encryptedCardNumber);
        if (card == null)
        {
            logger.LogError("The card with hash {encryptedCardNumber} was not found", encryptedCardNumber);
            throw new CardNotFoundException("Card not found!");
        }
        return card.Balance;
    }

    public async Task<(List<CardTransaction>, int)> GetTransactions(string encryptedCardNumber, int page, int pageSize)
    {
        return await cardRepository.GetPaginatedTransactionsAsync(encryptedCardNumber, page, pageSize);
    }

    public async Task<(CardTransaction, double)> Withdraw(string encryptedCardNumber, double amount, string idempotencyKey)
    {
        var card = await cardRepository.GetCardByHash(encryptedCardNumber);
        if (card == null)
        {
            logger.LogError("The card with hash {encryptedCardNumber} was not found", encryptedCardNumber);
            throw new CardNotFoundException("Card not found!");
        }
        
        // 🔹 Check if this Idempotency-Key was already used
        var existingTransaction = await cardRepository.GetCardTransactionByIdempotencyKey(idempotencyKey);
        if (existingTransaction != null)
        {   
            logger.LogWarning("Idempotency-Key {idempotencyKey} already used for transaction {transactionId}", idempotencyKey, existingTransaction.Id);
            logger.LogWarning("Skipping withdrawal. Returning existing transaction {transactionId}", existingTransaction.Id);
            throw new TxDuplicatedException("TransactionID: " + existingTransaction.Id);
        }

        if (card.Balance < amount)
        {
            logger.LogError("The card with hash {encryptedCardNumber} has insufficient funds to make the withdrawal", encryptedCardNumber);
            throw new InsufficientBalanceException("Insufficient funds!");
        }

        // 🔹 Process the transaction
        card.Balance -= amount;

        var transaction = new CardTransaction
        {
            Card = card,
            Amount = -amount,
            Date = DateTime.UtcNow,
            Type = TransactionType.Withdrawal,
            IdempotencyKey = idempotencyKey,
            AtmLocation = "Ezeiza Airport"
        };
        await cardRepository.AddTransactionAsync(transaction);
        await cardRepository.UpdateAsync(card);
        
        return (transaction, card.Balance);
    }
}