using AtmChallenge.Domain.Entities.Card;

namespace AtmChallenge.Application.Interfaces;

public interface ICardService
{
    Task<double> GetBalance(string encryptedCardNumber);
    Task<List<CardTransaction>> GetTransactions(string encryptedCardNumber);
    Task<CardTransaction> Withdraw(string encryptedCardNumber, double amount);
}