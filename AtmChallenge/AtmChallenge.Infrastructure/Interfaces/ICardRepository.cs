using AtmChallenge.Domain.Entities.Card;

namespace AtmChallenge.Infrastructure.Interfaces;

public interface ICardRepository
{
    Task<Card?> AuthenticateCardAsync(string numberHash, string pinHash);
    Task<Card?> GetCardByHash(string numberHash);

    Task<(List<CardTransaction>, int)>
        GetPaginatedTransactionsAsync(string encryptedCardNumber, int page, int pageSize);

    Task AddTransactionAsync(CardTransaction transaction);
    Task UpdateAsync(Card card);
    Task<CardTransaction?> GetCardTransactionByIdempotencyKey(string idempotencyKey);
}