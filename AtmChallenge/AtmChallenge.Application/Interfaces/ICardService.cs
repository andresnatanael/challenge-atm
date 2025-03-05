using AtmChallenge.Domain.Entities.Card;

namespace AtmChallenge.Application.Interfaces;

public interface ICardService
{
    Task<double> GetBalance(string encryptedCardNumber);
    Task<(List<CardTransaction>, int)> GetTransactions(string encryptedCardNumber, int page, int pageSize);
    Task<(CardTransaction, double)> Withdraw(string encryptedCardNumber, double amount, string idempotencyKey);
}